using FlightOps.Data;
using FlightOps.Entities;
using FlightOps.Enums;
using Microsoft.EntityFrameworkCore;

namespace FlightOps.Repositories.Flights;

public class FlightRepository(FlightOpsDbContext context) : IFlightRepository
{
    public async Task<List<Flight>> GetAllFlightsAsync() => await WithNavigationProperties(context.Flights)
        .AsNoTracking()
        .ToListAsync();

    public async Task<Flight?> GetFlightByIdAsync(int id) => await WithNavigationProperties(context.Flights)
        .AsNoTracking()
        .SingleOrDefaultAsync(f => f.Id == id);

    public async Task<Flight?> CreateFlightAsync(Flight flight)
    {
        await context.Flights.AddAsync(flight);
        await context.SaveChangesAsync();
        return flight;
    }

    public async Task<Flight?> UpdateFlightAsync(Flight flight)
    {
        context.Flights.Update(flight);
        await context.SaveChangesAsync();
        return flight;
    }

    public async Task<Flight?> DeleteFlightAsync(int id)
    {
        int deleted = await context.Flights
            .Where(f => f.Id == id)
            .ExecuteDeleteAsync();

        return deleted > 0 ? new Flight { Id = id } : null;
    }

    public async Task<List<Flight>> GetDepartedFlightsAsync() => await WithNavigationProperties(context.Flights)
        .AsNoTracking()
        .Where(f => f.Status == FlightStatus.Departed)
        .ToListAsync();

    public async Task<List<Flight>> GetBlockingFlightsForAircraftAsync(int aircraftId, int? excludeFlightId) =>
        await context.Flights.AsNoTracking()
            .Where(f => f.AircraftId == aircraftId
                && (f.Status == FlightStatus.Scheduled || f.Status == FlightStatus.Departed)
                && (excludeFlightId == null || f.Id != excludeFlightId))
            .ToListAsync();

    public async Task<Flight?> GetLastArrivedBeforeAsync(int aircraftId, DateTime at) =>
        await context.Flights.AsNoTracking()
            .Where(f => f.AircraftId == aircraftId
                && f.Status == FlightStatus.Arrived
                && f.ArrivalTime <= at)
            .OrderByDescending(f => f.ArrivalTime)
            .FirstOrDefaultAsync();

    public async Task<int> ApplyLifecycleTransitionsAsync(DateTime now)
    {
        int updated = 0;

        if (await context.Flights.AnyAsync(f =>
                f.Status == FlightStatus.Scheduled && now >= f.ArrivalTime))
        {
            updated += await context.Flights
                .Where(f => f.Status == FlightStatus.Scheduled && now >= f.ArrivalTime)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(f => f.Status, FlightStatus.Cancelled));
        }

        if (await context.Flights.AnyAsync(f =>
                f.Status == FlightStatus.Scheduled
                && f.DepartureTime <= now
                && now < f.ArrivalTime))
        {
            updated += await context.Flights
                .Where(f => f.Status == FlightStatus.Scheduled
                    && f.DepartureTime <= now
                    && now < f.ArrivalTime)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(f => f.Status, FlightStatus.Departed));
        }

        if (await context.Flights.AnyAsync(f =>
                f.Status == FlightStatus.Departed && now >= f.ArrivalTime))
        {
            updated += await context.Flights
                .Where(f => f.Status == FlightStatus.Departed && now >= f.ArrivalTime)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(f => f.Status, FlightStatus.Arrived));
        }

        return updated;
    }

    public async Task<List<Flight>> GetFlightsByAircraftIdAsync(int aircraftId, int take = 20) =>
        await WithNavigationProperties(context.Flights)
            .AsNoTracking()
            .Where(f => f.AircraftId == aircraftId)
            .OrderByDescending(f => f.DepartureTime)
            .Take(take)
            .ToListAsync();

    public async Task<List<Flight>> GetFlightsByAirportIdAsync(int airportId, int take = 20) =>
        await WithNavigationProperties(context.Flights)
            .AsNoTracking()
            .Where(f => f.OriginId == airportId || f.DestinationId == airportId)
            .OrderByDescending(f => f.DepartureTime)
            .Take(take)
            .ToListAsync();

    public async Task<FlightStatusCounts> GetDashboardCountsAsync(
        DateTime nowUtc,
        DateTime upcomingCutoffUtc,
        DateTime dayStartUtc,
        DateTime dayEndUtc)
    {
        FlightStatusCounts? counts = await context.Flights.AsNoTracking()
            .GroupBy(_ => 1)
            .Select(g => new FlightStatusCounts(
                g.Count(f => f.Status == FlightStatus.Departed),
                g.Count(f => f.Status == FlightStatus.Scheduled
                    && f.DepartureTime >= nowUtc
                    && f.DepartureTime <= upcomingCutoffUtc),
                g.Count(f => f.Status == FlightStatus.Cancelled
                    && f.DepartureTime >= dayStartUtc
                    && f.DepartureTime < dayEndUtc)))
            .SingleOrDefaultAsync();

        return counts ?? new FlightStatusCounts(0, 0, 0);
    }

    public async Task<List<Flight>> GetUpcomingScheduledFlightsAsync(DateTime now, int take) =>
        await WithNavigationProperties(context.Flights)
            .AsNoTracking()
            .Where(f => f.Status == FlightStatus.Scheduled && f.DepartureTime >= now)
            .OrderBy(f => f.DepartureTime)
            .Take(take)
            .ToListAsync();

    private static IQueryable<Flight> WithNavigationProperties(IQueryable<Flight> query) =>
        query
            .Include(f => f.Aircraft)
            .Include(f => f.Destination)
            .Include(f => f.Origin);
}
