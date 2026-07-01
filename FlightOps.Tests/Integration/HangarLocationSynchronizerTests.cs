using FlightOps.Entities;
using FlightOps.Enums;
using FlightOps.Features.Aircrafts;
using FlightOps.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace FlightOps.Tests.Integration;

public class HangarLocationSynchronizerTests : IDisposable
{
    private readonly TestDbContextFactory _factory = new();
    private readonly (Airport Origin, Airport Destination, Aircraft Aircraft) _fleet;
    private readonly DateTime _now = new(2026, 1, 1, 12, 0, 0);

    public HangarLocationSynchronizerTests()
    {
        _fleet = _factory.SeedMinimalFleet();
        Aircraft aircraft = _factory.Context.Aircrafts.Single(a => a.Id == _fleet.Aircraft.Id);
        aircraft.CurrentAirportId = _fleet.Origin.Id;
        _factory.Context.SaveChanges();
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task SyncAsync_AircraftInFlight_ClearsHangarLocation()
    {
        _factory.Context.Flights.Add(new Flight
        {
            AircraftId = _fleet.Aircraft.Id,
            OriginId = _fleet.Origin.Id,
            DestinationId = _fleet.Destination.Id,
            DepartureTime = _now.AddMinutes(-30),
            ArrivalTime = _now.AddMinutes(30),
            Status = FlightStatus.Departed,
            Distance = 277,
            Fuel = 1031
        });
        await _factory.Context.SaveChangesAsync();

        await HangarLocationSynchronizer.SyncAsync(_factory.Context, _now);

        int? hangar = await _factory.Context.Aircrafts.AsNoTracking()
            .Where(a => a.Id == _fleet.Aircraft.Id)
            .Select(a => a.CurrentAirportId)
            .SingleAsync();

        Assert.Null(hangar);
    }

    [Fact]
    public async Task SyncAsync_AfterArrival_MovesAircraftToDestination()
    {
        _factory.Context.Flights.Add(new Flight
        {
            AircraftId = _fleet.Aircraft.Id,
            OriginId = _fleet.Origin.Id,
            DestinationId = _fleet.Destination.Id,
            DepartureTime = _now.AddHours(-3),
            ArrivalTime = _now.AddHours(-1),
            Status = FlightStatus.Arrived,
            Distance = 277,
            Fuel = 1031
        });
        await _factory.Context.SaveChangesAsync();

        await HangarLocationSynchronizer.SyncAsync(_factory.Context, _now);

        int? hangar = await _factory.Context.Aircrafts.AsNoTracking()
            .Where(a => a.Id == _fleet.Aircraft.Id)
            .Select(a => a.CurrentAirportId)
            .SingleAsync();

        Assert.Equal(_fleet.Destination.Id, hangar);
    }

    [Fact]
    public async Task SyncAsync_MultipleArrivals_UsesMostRecentDestination()
    {
        Airport faro = new()
        {
            IATA = "FAO",
            Name = "Faro",
            City = "Faro",
            Country = "Portugal",
            Latitude = 37.0144,
            Longitude = -7.9659
        };
        _factory.Context.Airports.Add(faro);
        await _factory.Context.SaveChangesAsync();

        _factory.Context.Flights.AddRange(
            new Flight
            {
                AircraftId = _fleet.Aircraft.Id,
                OriginId = _fleet.Origin.Id,
                DestinationId = _fleet.Destination.Id,
                DepartureTime = _now.AddDays(-2),
                ArrivalTime = _now.AddDays(-2).AddHours(1),
                Status = FlightStatus.Arrived,
                Distance = 277,
                Fuel = 1031
            },
            new Flight
            {
                AircraftId = _fleet.Aircraft.Id,
                OriginId = _fleet.Destination.Id,
                DestinationId = faro.Id,
                DepartureTime = _now.AddDays(-1),
                ArrivalTime = _now.AddHours(-2),
                Status = FlightStatus.Arrived,
                Distance = 300,
                Fuel = 1100
            });
        await _factory.Context.SaveChangesAsync();

        await HangarLocationSynchronizer.SyncAsync(_factory.Context, _now);

        int? hangar = await _factory.Context.Aircrafts.AsNoTracking()
            .Where(a => a.Id == _fleet.Aircraft.Id)
            .Select(a => a.CurrentAirportId)
            .SingleAsync();

        Assert.Equal(faro.Id, hangar);
    }
}
