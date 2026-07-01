using FlightOps.Entities;
using FlightOps.Enums;
using FlightOps.Features.Flights.Scheduling;
using FlightOps.Infrastructure;
using FlightOps.Models.Pages.Home;
using FlightOps.Repositories.Aircrafts;
using FlightOps.Repositories.Flights;

namespace FlightOps.Features.Home.Queries;

public sealed class OperationsDashboardQuery(
    IFlightLifecycleApplier lifecycleApplier,
    IFlightRepository flightRepository,
    IAircraftRepository aircraftRepository,
    TimeProvider timeProvider,
    IUserTimeZoneAccessor userTimeZoneAccessor,
    IFlightTimeConverter flightTimeConverter) : IOperationsDashboardQuery
{
    private const int NextDeparturesLimit = 6;
    private static readonly TimeSpan UpcomingWindow = TimeSpan.FromHours(2);

    public async Task<OperationsDashboardModel> BuildAsync()
    {
        DateTime now = timeProvider.GetUtcNow().UtcDateTime;
        int offsetMinutes = userTimeZoneAccessor.GetOffsetMinutes();
        (DateTime dayStartUtc, DateTime dayEndUtc) = flightTimeConverter.GetUtcDayRange(offsetMinutes, now);
        await lifecycleApplier.ApplyTransitionsAsync(now);

        DateTime upcomingCutoff = now.Add(UpcomingWindow);

        FlightStatusCounts counts = await flightRepository.GetDashboardCountsAsync(
            now,
            upcomingCutoff,
            dayStartUtc,
            dayEndUtc);
        List<Flight> nextDepartures = await flightRepository.GetUpcomingScheduledFlightsAsync(now, NextDeparturesLimit);
        List<Aircraft> aircraft = await aircraftRepository.GetAllAircraftAsync();

        int airborne = aircraft.Count(a => a.CurrentAirportId is null);
        int onGround = aircraft.Count - airborne;

        return new OperationsDashboardModel
        {
            ActiveFlights = counts.ActiveFlights,
            AircraftOnGround = onGround,
            AircraftAirborne = airborne,
            UpcomingDeparturesCount = counts.UpcomingDeparturesCount,
            CancelledTodayCount = counts.CancelledTodayCount,
            NextDepartures = nextDepartures
                .Select(ToDepartureModel)
                .ToList(),
            FleetStatus = aircraft
                .Select(ToFleetItem)
                .OrderByDescending(item => item.IsInFlight)
                .ThenBy(item => item.Registration)
                .ToList()
        };
    }

    private static DashboardDepartureModel ToDepartureModel(Flight flight) => new()
    {
        FlightId = flight.Id,
        DepartureTime = flight.DepartureTime,
        OriginIata = flight.Origin?.IATA ?? "—",
        DestinationIata = flight.Destination?.IATA ?? "—",
        Registration = flight.Aircraft?.Registration ?? "—",
        Status = flight.Status
    };

    private static DashboardFleetItemModel ToFleetItem(Aircraft aircraft) => new()
    {
        AircraftId = aircraft.Id,
        Registration = aircraft.Registration ?? "—",
        Model = aircraft.Model ?? "",
        IsInFlight = aircraft.CurrentAirportId is null,
        LocationIata = aircraft.CurrentAirport?.IATA
    };
}
