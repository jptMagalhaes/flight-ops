using FlightOps.Entities;
using FlightOps.Enums;
using FlightOps.Repositories.Flights;
using FlightOps.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace FlightOps.Tests.Integration;

public class FlightLifecycleTransitionsTests : IDisposable
{
    private readonly TestDbContextFactory _factory = new();
    private readonly (Airport Origin, Airport Destination, Aircraft Aircraft) _fleet;
    private readonly DateTime _now = new(2026, 1, 1, 12, 0, 0);

    public FlightLifecycleTransitionsTests()
    {
        _fleet = _factory.SeedMinimalFleet();
    }

    public void Dispose() => _factory.Dispose();

    private Flight AddFlight(FlightStatus status, DateTime departure, DateTime arrival)
    {
        Flight flight = new()
        {
            AircraftId = _fleet.Aircraft.Id,
            OriginId = _fleet.Origin.Id,
            DestinationId = _fleet.Destination.Id,
            DepartureTime = departure,
            ArrivalTime = arrival,
            Status = status,
            Distance = 277,
            Fuel = 1031
        };
        _factory.Context.Flights.Add(flight);
        _factory.Context.SaveChanges();
        return flight;
    }

    // ApplyLifecycleTransitionsAsync uses ExecuteUpdateAsync, which issues a raw SQL UPDATE and
    // bypasses the change tracker. Reading back through a tracked query (e.g. FindAsync) would
    // return the stale in-memory entity instead of the real row, so this must be untracked.
    private async Task<FlightStatus> GetStatusAsync(int flightId) =>
        (await _factory.Context.Flights.AsNoTracking().SingleAsync(f => f.Id == flightId)).Status;

    [Fact]
    public async Task ScheduledFlight_DepartureTimeReached_BecomesDeparted()
    {
        Flight flight = AddFlight(FlightStatus.Scheduled, _now.AddMinutes(-10), _now.AddHours(1));
        FlightRepository repository = new(_factory.Context);

        await repository.ApplyLifecycleTransitionsAsync(_now);

        Assert.Equal(FlightStatus.Departed, await GetStatusAsync(flight.Id));
    }

    [Fact]
    public async Task DepartedFlight_ArrivalTimeReached_BecomesArrived()
    {
        Flight flight = AddFlight(FlightStatus.Departed, _now.AddHours(-2), _now.AddMinutes(-1));
        FlightRepository repository = new(_factory.Context);

        await repository.ApplyLifecycleTransitionsAsync(_now);

        Assert.Equal(FlightStatus.Arrived, await GetStatusAsync(flight.Id));
    }

    [Fact]
    public async Task ScheduledFlight_NeverDeparted_PastArrivalTime_BecomesCancelled()
    {
        Flight flight = AddFlight(FlightStatus.Scheduled, _now.AddHours(-3), _now.AddHours(-2));
        FlightRepository repository = new(_factory.Context);

        await repository.ApplyLifecycleTransitionsAsync(_now);

        Assert.Equal(FlightStatus.Cancelled, await GetStatusAsync(flight.Id));
    }

    [Fact]
    public async Task ScheduledFlight_BeforeDeparture_NoChange()
    {
        Flight flight = AddFlight(FlightStatus.Scheduled, _now.AddHours(1), _now.AddHours(2));
        FlightRepository repository = new(_factory.Context);

        await repository.ApplyLifecycleTransitionsAsync(_now);

        Assert.Equal(FlightStatus.Scheduled, await GetStatusAsync(flight.Id));
    }

    [Fact]
    public async Task MultipleFlights_SameTick_AllTransitionCorrectly()
    {
        Flight toDepart = AddFlight(FlightStatus.Scheduled, _now.AddMinutes(-5), _now.AddHours(1));
        Flight toArrive = AddFlight(FlightStatus.Departed, _now.AddHours(-2), _now.AddMinutes(-1));
        Flight toCancel = AddFlight(FlightStatus.Scheduled, _now.AddHours(-3), _now.AddHours(-1));
        Flight untouched = AddFlight(FlightStatus.Scheduled, _now.AddHours(2), _now.AddHours(3));

        FlightRepository repository = new(_factory.Context);
        int updatedCount = await repository.ApplyLifecycleTransitionsAsync(_now);

        Assert.Equal(3, updatedCount);
        Assert.Equal(FlightStatus.Departed, await GetStatusAsync(toDepart.Id));
        Assert.Equal(FlightStatus.Arrived, await GetStatusAsync(toArrive.Id));
        Assert.Equal(FlightStatus.Cancelled, await GetStatusAsync(toCancel.Id));
        Assert.Equal(FlightStatus.Scheduled, await GetStatusAsync(untouched.Id));
    }
}
