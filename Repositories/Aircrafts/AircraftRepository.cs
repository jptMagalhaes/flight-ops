using FlightOps.Data;
using FlightOps.Entities;
using FlightOps.Features.Aircrafts;
using Microsoft.EntityFrameworkCore;

namespace FlightOps.Repositories.Aircrafts;

public class AircraftRepository(FlightOpsDbContext context) : IAircraftRepository
{
    public async Task<List<Aircraft>> GetAllAircraftAsync() => await context.Aircrafts.AsNoTracking()
        .Include(p => p.CurrentAirport)
        .OrderBy(p => p.Registration)
        .ToListAsync();

    public async Task<Aircraft?> GetAircraftByIdAsync(int id) => await context.Aircrafts.AsNoTracking()
        .Include(p => p.CurrentAirport)
        .SingleOrDefaultAsync(p => p.Id == id);

    public async Task<Aircraft?> CreateAircraftAsync(Aircraft aircraft)
    {
        await context.Aircrafts.AddAsync(aircraft);
        await context.SaveChangesAsync();
        return aircraft;
    }

    public async Task<Aircraft?> UpdateAircraftAsync(Aircraft aircraft)
    {
        context.Aircrafts.Update(aircraft);
        await context.SaveChangesAsync();
        return aircraft;
    }

    public async Task<Aircraft?> DeleteAircraftAsync(int id)
    {
        Aircraft? aircraft = await GetAircraftByIdAsync(id);
        if (aircraft is null)
            return null;

        context.Aircrafts.Remove(aircraft);
        await context.SaveChangesAsync();
        return aircraft;
    }

    public async Task SyncHangarLocationsAsync(DateTime now) =>
        await HangarLocationSynchronizer.SyncAsync(context, now);

    public async Task<List<Aircraft>> GetAircraftAtAirportAsync(int airportId) =>
        await context.Aircrafts.AsNoTracking()
            .Include(p => p.CurrentAirport)
            .Where(p => p.CurrentAirportId == airportId)
            .OrderBy(p => p.Registration)
            .ToListAsync();
}
