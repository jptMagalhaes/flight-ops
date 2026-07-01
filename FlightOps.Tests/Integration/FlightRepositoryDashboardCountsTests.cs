using FlightOps.Entities;
using FlightOps.Enums;
using FlightOps.Infrastructure;
using FlightOps.Repositories.Flights;
using FlightOps.Tests.Support;

namespace FlightOps.Tests.Integration;

public class FlightRepositoryDashboardCountsTests : IDisposable
{
    private readonly TestDbContextFactory _factory = new();

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task GetDashboardCountsAsync_UsesLocalDayRangeFromOffset()
    {
        (Airport origin, Airport destination, Aircraft aircraft) = _factory.SeedMinimalFleet();

        _factory.Context.Flights.AddRange(
            BuildFlight(origin.Id, destination.Id, aircraft.Id, FlightStatus.Cancelled, new DateTime(2026, 6, 30, 23, 30, 0, DateTimeKind.Utc)),
            BuildFlight(origin.Id, destination.Id, aircraft.Id, FlightStatus.Cancelled, new DateTime(2026, 7, 1, 23, 30, 0, DateTimeKind.Utc)));
        await _factory.Context.SaveChangesAsync();

        FlightRepository repository = new(_factory.Context);
        IFlightTimeConverter converter = new FlightTimeConverter();
        DateTime nowUtc = new(2026, 7, 1, 0, 30, 0, DateTimeKind.Utc);
        (DateTime dayStartUtc, DateTime dayEndUtc) = converter.GetUtcDayRange(-60, nowUtc);

        FlightStatusCounts counts = await repository.GetDashboardCountsAsync(
            nowUtc,
            nowUtc.AddHours(2),
            dayStartUtc,
            dayEndUtc);

        Assert.Equal(1, counts.CancelledTodayCount);
    }

    private static Flight BuildFlight(
        int originId,
        int destinationId,
        int aircraftId,
        FlightStatus status,
        DateTime departureUtc) => new()
    {
        OriginId = originId,
        DestinationId = destinationId,
        AircraftId = aircraftId,
        DepartureTime = departureUtc,
        ArrivalTime = departureUtc.AddHours(1),
        Distance = 100,
        Fuel = 500,
        Status = status
    };
}
