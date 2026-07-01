using FlightOps.Entities;
using FlightOps.Enums;
using FlightOps.Features.Flights.Commands;
using FlightOps.Features.Flights.Scheduling;
using FlightOps.Repositories.Aircrafts;
using FlightOps.Repositories.Flights;
using FlightOps.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace FlightOps.Tests.Integration;

public class FlightCommandsTests : IDisposable
{
    private readonly FlightCommandsTestContext _ctx = new();
    private readonly DateTime _departure = new(2026, 7, 1, 10, 0, 0);

    public void Dispose() => _ctx.Dispose();

    private Flight ValidCandidate(FlightStatus status = FlightStatus.Scheduled) => new()
    {
        AircraftId = _ctx.Fleet.Aircraft.Id,
        OriginId = _ctx.Fleet.Origin.Id,
        DestinationId = _ctx.Fleet.Destination.Id,
        DepartureTime = _departure,
        Status = status
    };

    [Fact]
    public async Task CreateFlightAsync_SameOriginAndDestination_ReturnsInvalidRoute()
    {
        Flight candidate = ValidCandidate();
        candidate.DestinationId = candidate.OriginId;

        FlightOperationResult<Flight> result = await _ctx.Commands.CreateFlightAsync(candidate);

        Assert.False(result.IsSuccess);
        Assert.Equal(FlightCommandsError.InvalidRoute, result.Error);
    }

    [Fact]
    public async Task CreateFlightAsync_ValidScheduledFlight_RecalculatesMetricsAndSucceeds()
    {
        Flight candidate = ValidCandidate();

        FlightOperationResult<Flight> result = await _ctx.Commands.CreateFlightAsync(candidate);

        Assert.True(result.IsSuccess);
        Assert.InRange(result.Value!.Distance, 270, 285);
        Assert.True(result.Value.Fuel > 200);
        Assert.True(result.Value.ArrivalTime > candidate.DepartureTime);
    }

    [Fact]
    public async Task CreateFlightAsync_OverlappingSchedule_ReturnsConflict()
    {
        Flight first = ValidCandidate();
        Assert.True((await _ctx.Commands.CreateFlightAsync(first)).IsSuccess);

        Flight overlapping = ValidCandidate();
        overlapping.DepartureTime = _departure.AddMinutes(10);

        FlightOperationResult<Flight> result = await _ctx.Commands.CreateFlightAsync(overlapping);

        Assert.False(result.IsSuccess);
        Assert.Equal(FlightCommandsError.AircraftScheduleConflict, result.Error);
    }

    [Fact]
    public async Task CreateFlightAsync_WrongOrigin_ReturnsAircraftWrongOrigin()
    {
        _ctx.ParkAircraftAt(_ctx.Fleet.Aircraft.Id, _ctx.Fleet.Destination.Id);

        FlightOperationResult<Flight> result = await _ctx.Commands.CreateFlightAsync(ValidCandidate());

        Assert.False(result.IsSuccess);
        Assert.Equal(FlightCommandsError.AircraftWrongOrigin, result.Error);
    }

    [Fact]
    public async Task CreateFlightAsync_ManualDepart_SetsDepartureToNow()
    {
        DateTime expectedDeparture = _ctx.TimeProvider.GetUtcNow().UtcDateTime;
        Flight candidate = ValidCandidate(FlightStatus.Departed);

        FlightOperationResult<Flight> result = await _ctx.Commands.CreateFlightAsync(candidate);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedDeparture, result.Value!.DepartureTime);
    }

    [Fact]
    public async Task UpdateFlightAsync_ArrivedFlight_ReturnsInvalidStatusTransition()
    {
        FlightOperationResult<Flight> created =
            await _ctx.Commands.CreateFlightAsync(ValidCandidate(FlightStatus.Departed));
        Assert.True(created.IsSuccess);

        await _ctx.Context.Flights
            .Where(f => f.Id == created.Value!.Id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(f => f.ArrivalTime, _ctx.TimeProvider.GetUtcNow().UtcDateTime.AddMinutes(-5)));

        FlightLifecycleApplier lifecycleApplier = new(
            new FlightRepository(_ctx.Context),
            new AircraftRepository(_ctx.Context),
            _ctx.TimeProvider);
        await lifecycleApplier.ApplyTransitionsAsync(_ctx.TimeProvider.GetUtcNow().UtcDateTime);

        _ctx.Context.ChangeTracker.Clear();
        Flight? arrived = await _ctx.Commands.GetFlightByIdAsync(created.Value!.Id);
        Assert.NotNull(arrived);
        Assert.Equal(FlightStatus.Arrived, arrived!.Status);

        arrived.DepartureTime = arrived.DepartureTime.AddHours(-1);
        FlightOperationResult<Flight> result = await _ctx.Commands.UpdateFlightAsync(arrived);

        Assert.False(result.IsSuccess);
        Assert.Equal(FlightCommandsError.InvalidStatusTransition, result.Error);
    }

    [Fact]
    public async Task UpdateFlightAsync_ScheduledToDeparted_Allowed()
    {
        FlightOperationResult<Flight> created = await _ctx.Commands.CreateFlightAsync(ValidCandidate());
        Assert.True(created.IsSuccess);

        _ctx.Context.ChangeTracker.Clear();
        Flight? loaded = await _ctx.Commands.GetFlightByIdAsync(created.Value!.Id);
        Assert.NotNull(loaded);
        loaded!.Status = FlightStatus.Departed;
        DateTime expectedDeparture = _ctx.TimeProvider.GetUtcNow().UtcDateTime;

        FlightOperationResult<Flight> result = await _ctx.Commands.UpdateFlightAsync(loaded);

        Assert.True(result.IsSuccess);
        Assert.Equal(FlightStatus.Departed, result.Value!.Status);
        Assert.Equal(expectedDeparture, result.Value.DepartureTime);
    }

    [Fact]
    public async Task DeleteFlightAsync_ExistingFlight_ReturnsDeletedStub()
    {
        FlightOperationResult<Flight> created =
            await _ctx.Commands.CreateFlightAsync(ValidCandidate());
        Assert.True(created.IsSuccess);

        Flight? deleted = await _ctx.Commands.DeleteFlightAsync(created.Value!.Id);

        Assert.NotNull(deleted);
        Assert.Equal(created.Value.Id, deleted!.Id);
        Assert.Null(await _ctx.Commands.GetFlightByIdAsync(created.Value.Id));
    }
}
