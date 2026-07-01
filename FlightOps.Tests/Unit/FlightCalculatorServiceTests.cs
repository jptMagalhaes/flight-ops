using FlightOps.Features.Flights.Domain;

namespace FlightOps.Tests.Unit;

public class FlightCalculatorServiceTests
{
    [Fact]
    public void CalculateDistanceKm_LisbonToPorto_ReturnsExpectedDistance()
    {
        double distance = FlightCalculatorService.CalculateDistanceKm(
            38.7742, -9.1342,
            41.2481, -8.6814);

        Assert.InRange(distance, 270, 285);
    }

    [Fact]
    public void CalculateFuel_ReturnsDistanceTimesConsumptionPlusTakeoffEffort()
    {
        double fuel = FlightCalculatorService.CalculateFuel(
            distanceKm: 300,
            fuelConsumptionPerKm: 3.0,
            takeoffEffort: 200);

        Assert.Equal(300 * 3.0 + 200, fuel);
    }

    [Fact]
    public void CalculateFlightDuration_ReturnsDistanceOverCruiseSpeed()
    {
        TimeSpan duration = FlightCalculatorService.CalculateFlightDuration(
            distanceKm: 800,
            cruiseSpeedKmh: 800);

        Assert.Equal(TimeSpan.FromHours(1), duration);
    }

    [Fact]
    public void CalculateFlightDuration_ZeroCruiseSpeed_Throws()
    {
        // CalculateFlightDuration has no guard of its own — callers (FlightCalculationPreviewQuery)
        // are responsible for rejecting CruiseSpeedKmh <= 0 before calling it. This test documents
        // that contract so a future caller doesn't assume the method is safe to call unguarded.
        Assert.Throws<OverflowException>(() =>
            FlightCalculatorService.CalculateFlightDuration(distanceKm: 100, cruiseSpeedKmh: 0));
    }

    [Fact]
    public void CalculateFlightMetrics_ComposesDistanceFuelAndArrival()
    {
        DateTime departure = new(2026, 5, 1, 8, 0, 0);

        FlightMetrics metrics = FlightCalculatorService.CalculateFlightMetrics(
            38.7742, -9.1342,
            41.2481, -8.6814,
            fuelConsumptionPerKm: 3.0,
            takeoffEffort: 200,
            cruiseSpeedKmh: 800,
            departure);

        Assert.InRange(metrics.Distance, 270, 285);
        Assert.Equal(metrics.Distance * 3.0 + 200, metrics.Fuel);
        Assert.Equal(departure + TimeSpan.FromHours(metrics.Distance / 800), metrics.ArrivalTime);
    }

    [Fact]
    public void CalculateDistanceKm_SamePoint_ReturnsZero()
    {
        double distance = FlightCalculatorService.CalculateDistanceKm(
            38.7742, -9.1342,
            38.7742, -9.1342);

        Assert.Equal(0, distance, precision: 3);
    }
}
