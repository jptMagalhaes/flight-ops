using FlightOps.Entities;
using FlightOps.Enums;
using FlightOps.Features.Flights.Scheduling;
using FlightOps.Repositories.Aircrafts;
using FlightOps.Repositories.Flights;
using Moq;

namespace FlightOps.Tests.Unit;

public class AircraftLocationResolverTests
{
    private static readonly DateTime At = new(2026, 6, 1, 12, 0, 0);

    [Fact]
    public async Task ResolveAirportIdAsync_LastArrivedFlight_ReturnsDestination()
    {
        Flight lastArrived = new()
        {
            AircraftId = 1,
            DestinationId = 42,
            Status = FlightStatus.Arrived,
            ArrivalTime = At.AddHours(-1)
        };

        Mock<IFlightRepository> flightRepository = new();
        flightRepository
            .Setup(r => r.GetLastArrivedBeforeAsync(1, At))
            .ReturnsAsync(lastArrived);
        Mock<IAircraftRepository> aircraftRepository = new();

        AircraftLocationResolver resolver = new(flightRepository.Object, aircraftRepository.Object);

        int? airportId = await resolver.ResolveAirportIdAsync(1, At);

        Assert.Equal(42, airportId);
        aircraftRepository.Verify(r => r.GetAircraftByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ResolveAirportIdAsync_NoArrivedFlight_FallsBackToHangarLocation()
    {
        Mock<IFlightRepository> flightRepository = new();
        flightRepository
            .Setup(r => r.GetLastArrivedBeforeAsync(1, At))
            .ReturnsAsync((Flight?)null);
        Mock<IAircraftRepository> aircraftRepository = new();
        aircraftRepository
            .Setup(r => r.GetAircraftByIdAsync(1))
            .ReturnsAsync(new Aircraft { Id = 1, CurrentAirportId = 7 });

        AircraftLocationResolver resolver = new(flightRepository.Object, aircraftRepository.Object);

        int? airportId = await resolver.ResolveAirportIdAsync(1, At);

        Assert.Equal(7, airportId);
    }

    [Fact]
    public async Task ResolveAirportIdAsync_NoHistoryAndNoHangar_ReturnsNull()
    {
        Mock<IFlightRepository> flightRepository = new();
        flightRepository
            .Setup(r => r.GetLastArrivedBeforeAsync(1, At))
            .ReturnsAsync((Flight?)null);
        Mock<IAircraftRepository> aircraftRepository = new();
        aircraftRepository
            .Setup(r => r.GetAircraftByIdAsync(1))
            .ReturnsAsync(new Aircraft { Id = 1, CurrentAirportId = null });

        AircraftLocationResolver resolver = new(flightRepository.Object, aircraftRepository.Object);

        int? airportId = await resolver.ResolveAirportIdAsync(1, At);

        Assert.Null(airportId);
    }
}
