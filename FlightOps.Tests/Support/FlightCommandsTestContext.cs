using FlightOps.Data;
using FlightOps.Entities;
using FlightOps.Features.Flights.Commands;
using FlightOps.Features.Flights.Scheduling;
using FlightOps.Repositories.Aircrafts;
using FlightOps.Repositories.Airports;
using FlightOps.Repositories.Flights;
using Microsoft.Extensions.Logging.Abstractions;

namespace FlightOps.Tests.Support;

/// <summary>
/// Wires FlightCommands against a real SQLite in-memory database with production repositories.
/// </summary>
public sealed class FlightCommandsTestContext : IDisposable
{
    private readonly TestDbContextFactory _factory;

    public FlightCommands Commands { get; }
    public FixedTimeProvider TimeProvider { get; }
    public FlightOpsDbContext Context => _factory.Context;
    public (Airport Origin, Airport Destination, Aircraft Aircraft) Fleet { get; }

    public FlightCommandsTestContext()
    {
        _factory = new TestDbContextFactory();
        TimeProvider = new FixedTimeProvider(new DateTimeOffset(2026, 6, 30, 12, 0, 0, TimeSpan.Zero));
        Fleet = _factory.SeedMinimalFleet();
        ParkAircraftAt(Fleet.Aircraft.Id, Fleet.Origin.Id);

        FlightRepository flightRepository = new(_factory.Context);
        AirportRepository airportRepository = new(_factory.Context);
        AircraftRepository aircraftRepository = new(_factory.Context);
        AircraftLocationResolver locationResolver = new(flightRepository, aircraftRepository);
        FlightScheduleValidator scheduleValidator = new(flightRepository, locationResolver);
        FlightLifecycleApplier lifecycleApplier = new(flightRepository, aircraftRepository, TimeProvider);

        Commands = new FlightCommands(
            NullLogger<FlightCommands>.Instance,
            flightRepository,
            airportRepository,
            lifecycleApplier,
            scheduleValidator,
            aircraftRepository,
            TimeProvider);
    }

    public void ParkAircraftAt(int aircraftId, int airportId)
    {
        Aircraft aircraft = _factory.Context.Aircrafts.Single(a => a.Id == aircraftId);
        aircraft.CurrentAirportId = airportId;
        _factory.Context.SaveChanges();
    }

    public void Dispose() => _factory.Dispose();
}
