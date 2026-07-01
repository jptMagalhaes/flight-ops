using FlightOps.Entities;
using FlightOps.Features.Flights.Queries;
using FlightOps.Models.Pages.Flights;
using FlightOps.Repositories.Aircrafts;
using FlightOps.Repositories.Airports;
using Moq;

namespace FlightOps.Tests.Unit;

public class FlightCalculationPreviewQueryTests
{
    private static readonly DateTime Departure = new(2026, 3, 15, 14, 0, 0);

    [Fact]
    public async Task BuildAsync_ValidRoute_ReturnsMetrics()
    {
        Mock<IAirportRepository> airportRepository = new();
        airportRepository.Setup(r => r.GetAirportByIdAsync(1))
            .ReturnsAsync(new Airport { Id = 1, Latitude = 38.7742, Longitude = -9.1342 });
        airportRepository.Setup(r => r.GetAirportByIdAsync(2))
            .ReturnsAsync(new Airport { Id = 2, Latitude = 41.2481, Longitude = -8.6814 });
        Mock<IAircraftRepository> aircraftRepository = new();
        aircraftRepository.Setup(r => r.GetAircraftByIdAsync(3))
            .ReturnsAsync(new Aircraft
            {
                Id = 3,
                FuelConsumptionPerKm = 3,
                TakeOffEffort = 200,
                CruiseSpeedKmh = 800
            });

        FlightCalculationPreviewQuery query = new(airportRepository.Object, aircraftRepository.Object);

        FlightCalculationPreviewModel? preview = await query.BuildAsync(1, 2, 3, Departure);

        Assert.NotNull(preview);
        Assert.InRange(preview!.Distance, 270, 285);
        Assert.True(preview.Fuel > 200);
        Assert.True(preview.ArrivalTime > Departure);
    }

    [Theory]
    [InlineData(0, 2, 3)]
    [InlineData(1, 1, 3)]
    [InlineData(1, 2, 0)]
    public async Task BuildAsync_InvalidIds_ReturnsNull(int originId, int destinationId, int aircraftId)
    {
        Mock<IAirportRepository> airportRepository = new();
        Mock<IAircraftRepository> aircraftRepository = new();
        FlightCalculationPreviewQuery query = new(airportRepository.Object, aircraftRepository.Object);

        FlightCalculationPreviewModel? preview =
            await query.BuildAsync(originId, destinationId, aircraftId, Departure);

        Assert.Null(preview);
        airportRepository.Verify(r => r.GetAirportByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task BuildAsync_ZeroCruiseSpeed_ReturnsNull()
    {
        Mock<IAirportRepository> airportRepository = new();
        airportRepository.Setup(r => r.GetAirportByIdAsync(1))
            .ReturnsAsync(new Airport { Id = 1, Latitude = 38.7742, Longitude = -9.1342 });
        airportRepository.Setup(r => r.GetAirportByIdAsync(2))
            .ReturnsAsync(new Airport { Id = 2, Latitude = 41.2481, Longitude = -8.6814 });
        Mock<IAircraftRepository> aircraftRepository = new();
        aircraftRepository.Setup(r => r.GetAircraftByIdAsync(3))
            .ReturnsAsync(new Aircraft { Id = 3, CruiseSpeedKmh = 0 });

        FlightCalculationPreviewQuery query = new(airportRepository.Object, aircraftRepository.Object);

        FlightCalculationPreviewModel? preview = await query.BuildAsync(1, 2, 3, Departure);

        Assert.Null(preview);
    }
}
