using FlightOps.Entities;
using FlightOps.Enums;
using FlightOps.Features.Flights.Scheduling;
using FlightOps.Repositories.Flights;
using Moq;

namespace FlightOps.Tests.Unit;

public class FlightScheduleValidatorTests
{
    private static Flight Candidate(FlightStatus status = FlightStatus.Scheduled, int aircraftId = 1, int originId = 1) => new()
    {
        AircraftId = aircraftId,
        OriginId = originId,
        DestinationId = 2,
        DepartureTime = new DateTime(2026, 1, 1, 10, 0, 0),
        ArrivalTime = new DateTime(2026, 1, 1, 11, 0, 0),
        Status = status
    };

    [Fact]
    public async Task ValidateAsync_NoBlockingFlights_ReturnsNull()
    {
        Mock<IFlightRepository> flightRepository = new();
        flightRepository
            .Setup(r => r.GetBlockingFlightsForAircraftAsync(It.IsAny<int>(), It.IsAny<int?>()))
            .ReturnsAsync([]);
        Mock<IAircraftLocationResolver> locationResolver = new();

        FlightScheduleValidator validator = new(flightRepository.Object, locationResolver.Object);

        FlightCommandsError? result = await validator.ValidateAsync(Candidate(), excludeFlightId: null, validateOrigin: false);

        Assert.Null(result);
    }

    [Fact]
    public async Task ValidateAsync_OverlappingFlight_ReturnsAircraftScheduleConflict()
    {
        Flight blocking = new()
        {
            DepartureTime = new DateTime(2026, 1, 1, 10, 30, 0),
            ArrivalTime = new DateTime(2026, 1, 1, 11, 30, 0)
        };
        Mock<IFlightRepository> flightRepository = new();
        flightRepository
            .Setup(r => r.GetBlockingFlightsForAircraftAsync(It.IsAny<int>(), It.IsAny<int?>()))
            .ReturnsAsync([blocking]);
        Mock<IAircraftLocationResolver> locationResolver = new();

        FlightScheduleValidator validator = new(flightRepository.Object, locationResolver.Object);

        FlightCommandsError? result = await validator.ValidateAsync(Candidate(), excludeFlightId: null, validateOrigin: false);

        Assert.Equal(FlightCommandsError.AircraftScheduleConflict, result);
    }

    [Fact]
    public async Task ValidateAsync_OriginDoesNotMatchResolvedLocation_ReturnsAircraftWrongOrigin()
    {
        Mock<IFlightRepository> flightRepository = new();
        flightRepository
            .Setup(r => r.GetBlockingFlightsForAircraftAsync(It.IsAny<int>(), It.IsAny<int?>()))
            .ReturnsAsync([]);
        Mock<IAircraftLocationResolver> locationResolver = new();
        locationResolver
            .Setup(r => r.ResolveAirportIdAsync(It.IsAny<int>(), It.IsAny<DateTime>()))
            .ReturnsAsync(99); // aircraft is actually at airport 99, candidate claims origin 1

        FlightScheduleValidator validator = new(flightRepository.Object, locationResolver.Object);

        FlightCommandsError? result = await validator.ValidateAsync(Candidate(originId: 1), excludeFlightId: null, validateOrigin: true);

        Assert.Equal(FlightCommandsError.AircraftWrongOrigin, result);
    }

    [Fact]
    public async Task ValidateAsync_ValidateOriginFalse_IgnoresOriginMismatch()
    {
        Mock<IFlightRepository> flightRepository = new();
        flightRepository
            .Setup(r => r.GetBlockingFlightsForAircraftAsync(It.IsAny<int>(), It.IsAny<int?>()))
            .ReturnsAsync([]);
        Mock<IAircraftLocationResolver> locationResolver = new();
        locationResolver
            .Setup(r => r.ResolveAirportIdAsync(It.IsAny<int>(), It.IsAny<DateTime>()))
            .ReturnsAsync(99);

        FlightScheduleValidator validator = new(flightRepository.Object, locationResolver.Object);

        FlightCommandsError? result = await validator.ValidateAsync(Candidate(originId: 1), excludeFlightId: null, validateOrigin: false);

        Assert.Null(result);
        locationResolver.Verify(r => r.ResolveAirportIdAsync(It.IsAny<int>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_StatusArrived_SkipsValidationEntirely()
    {
        Mock<IFlightRepository> flightRepository = new();
        Mock<IAircraftLocationResolver> locationResolver = new();

        FlightScheduleValidator validator = new(flightRepository.Object, locationResolver.Object);

        FlightCommandsError? result = await validator.ValidateAsync(
            Candidate(status: FlightStatus.Arrived), excludeFlightId: null, validateOrigin: true);

        Assert.Null(result);
        flightRepository.Verify(
            r => r.GetBlockingFlightsForAircraftAsync(It.IsAny<int>(), It.IsAny<int?>()), Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_PassesExcludeFlightIdThroughToRepository()
    {
        Mock<IFlightRepository> flightRepository = new();
        flightRepository
            .Setup(r => r.GetBlockingFlightsForAircraftAsync(It.IsAny<int>(), It.IsAny<int?>()))
            .ReturnsAsync([]);
        Mock<IAircraftLocationResolver> locationResolver = new();

        FlightScheduleValidator validator = new(flightRepository.Object, locationResolver.Object);

        await validator.ValidateAsync(Candidate(), excludeFlightId: 42, validateOrigin: false);

        flightRepository.Verify(
            r => r.GetBlockingFlightsForAircraftAsync(It.IsAny<int>(), 42), Times.Once);
    }

    [Fact]
    public void IntervalsOverlap_AdjacentFlights_DoNotOverlap()
    {
        // Regression guard: confirmed during code review that the half-open-interval formula is
        // correct (B departing exactly when A arrives is allowed, no buffer time required).
        bool overlaps = FlightScheduleValidator.IntervalsOverlap(
            departureA: new DateTime(2026, 1, 1, 10, 0, 0),
            arrivalA: new DateTime(2026, 1, 1, 11, 0, 0),
            departureB: new DateTime(2026, 1, 1, 11, 0, 0),
            arrivalB: new DateTime(2026, 1, 1, 12, 0, 0));

        Assert.False(overlaps);
    }

    [Fact]
    public void IntervalsOverlap_IdenticalWindows_Overlap()
    {
        bool overlaps = FlightScheduleValidator.IntervalsOverlap(
            departureA: new DateTime(2026, 1, 1, 10, 0, 0),
            arrivalA: new DateTime(2026, 1, 1, 11, 0, 0),
            departureB: new DateTime(2026, 1, 1, 10, 0, 0),
            arrivalB: new DateTime(2026, 1, 1, 11, 0, 0));

        Assert.True(overlaps);
    }

    [Fact]
    public void IntervalsOverlap_PartialOverlap_ReturnsTrue()
    {
        bool overlaps = FlightScheduleValidator.IntervalsOverlap(
            departureA: new DateTime(2026, 1, 1, 10, 0, 0),
            arrivalA: new DateTime(2026, 1, 1, 11, 0, 0),
            departureB: new DateTime(2026, 1, 1, 10, 30, 0),
            arrivalB: new DateTime(2026, 1, 1, 11, 30, 0));

        Assert.True(overlaps);
    }

    [Fact]
    public async Task ValidateAsync_CancelledStatus_SkipsAllChecks()
    {
        Mock<IFlightRepository> flightRepository = new();
        Mock<IAircraftLocationResolver> locationResolver = new();
        FlightScheduleValidator validator = new(flightRepository.Object, locationResolver.Object);

        FlightCommandsError? result = await validator.ValidateAsync(
            Candidate(status: FlightStatus.Cancelled), excludeFlightId: null, validateOrigin: true);

        Assert.Null(result);
        flightRepository.Verify(
            r => r.GetBlockingFlightsForAircraftAsync(It.IsAny<int>(), It.IsAny<int?>()), Times.Never);
    }
}
