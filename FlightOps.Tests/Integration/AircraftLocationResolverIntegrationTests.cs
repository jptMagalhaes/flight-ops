using FlightOps.Entities;
using FlightOps.Enums;
using FlightOps.Features.Flights.Scheduling;
using FlightOps.Repositories.Aircrafts;
using FlightOps.Repositories.Flights;
using FlightOps.Tests.Support;

namespace FlightOps.Tests.Integration;

public class AircraftLocationResolverIntegrationTests : IDisposable
{
    private readonly TestDbContextFactory _factory = new();
    private readonly (Airport Origin, Airport Destination, Aircraft Aircraft) _fleet;

    public AircraftLocationResolverIntegrationTests()
    {
        _fleet = _factory.SeedMinimalFleet();
        Aircraft aircraft = _factory.Context.Aircrafts.Single(a => a.Id == _fleet.Aircraft.Id);
        aircraft.CurrentAirportId = _fleet.Origin.Id;
        _factory.Context.SaveChanges();
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task ResolveAirportIdAsync_NoFlightHistory_ReturnsHangarAirport()
    {
        FlightRepository flightRepository = new(_factory.Context);
        AircraftRepository aircraftRepository = new(_factory.Context);
        AircraftLocationResolver resolver = new(flightRepository, aircraftRepository);
        DateTime at = new(2026, 1, 1, 8, 0, 0);

        int? airportId = await resolver.ResolveAirportIdAsync(_fleet.Aircraft.Id, at);

        Assert.Equal(_fleet.Origin.Id, airportId);
    }

    [Fact]
    public async Task ResolveAirportIdAsync_AfterArrivedFlight_ReturnsLastDestination()
    {
        DateTime arrival = new(2026, 1, 1, 9, 0, 0);
        _factory.Context.Flights.Add(new Flight
        {
            AircraftId = _fleet.Aircraft.Id,
            OriginId = _fleet.Origin.Id,
            DestinationId = _fleet.Destination.Id,
            DepartureTime = arrival.AddHours(-1),
            ArrivalTime = arrival,
            Status = FlightStatus.Arrived,
            Distance = 277,
            Fuel = 1031
        });
        await _factory.Context.SaveChangesAsync();

        FlightRepository flightRepository = new(_factory.Context);
        AircraftRepository aircraftRepository = new(_factory.Context);
        AircraftLocationResolver resolver = new(flightRepository, aircraftRepository);

        int? airportId = await resolver.ResolveAirportIdAsync(_fleet.Aircraft.Id, arrival.AddMinutes(1));

        Assert.Equal(_fleet.Destination.Id, airportId);
    }
}
