using FlightOps.Features.Flights.Simulation;

namespace FlightOps.Tests.Unit;

public class GeoInterpolationServiceTests
{
    private const double LisLat = 38.7742, LisLon = -9.1342;
    private const double OpoLat = 41.2481, OpoLon = -8.6814;

    [Fact]
    public void Fraction0_ReturnsOrigin()
    {
        (double lat, double lon) = GeoInterpolationService.InterpolateGreatCircle(
            LisLat, LisLon, OpoLat, OpoLon, fraction: 0);

        Assert.Equal(LisLat, lat, precision: 6);
        Assert.Equal(LisLon, lon, precision: 6);
    }

    [Fact]
    public void Fraction1_ReturnsDestination()
    {
        (double lat, double lon) = GeoInterpolationService.InterpolateGreatCircle(
            LisLat, LisLon, OpoLat, OpoLon, fraction: 1);

        Assert.Equal(OpoLat, lat, precision: 6);
        Assert.Equal(OpoLon, lon, precision: 6);
    }

    [Fact]
    public void Midpoint_IsBetweenOriginAndDestination()
    {
        (double lat, double lon) = GeoInterpolationService.InterpolateGreatCircle(
            LisLat, LisLon, OpoLat, OpoLon, fraction: 0.5);

        Assert.InRange(lat, Math.Min(LisLat, OpoLat), Math.Max(LisLat, OpoLat));
        Assert.InRange(lon, Math.Min(LisLon, OpoLon), Math.Max(LisLon, OpoLon));
    }

    [Theory]
    [InlineData(-0.5, 0)]
    [InlineData(1.5, 1)]
    public void FractionOutOfRange_ClampedToEndpoints(double fraction, double expectedFractionBehavior)
    {
        (double latAt0, _) = GeoInterpolationService.InterpolateGreatCircle(
            LisLat, LisLon, OpoLat, OpoLon, fraction: 0);
        (double latAt1, _) = GeoInterpolationService.InterpolateGreatCircle(
            LisLat, LisLon, OpoLat, OpoLon, fraction: 1);
        (double latClamped, _) = GeoInterpolationService.InterpolateGreatCircle(
            LisLat, LisLon, OpoLat, OpoLon, fraction: fraction);

        double expectedLat = expectedFractionBehavior < 0.5 ? latAt0 : latAt1;
        Assert.Equal(expectedLat, latClamped, precision: 6);
    }

    [Fact]
    public void SamePoint_ReturnsOriginForAnyFraction()
    {
        (double lat, double lon) = GeoInterpolationService.InterpolateGreatCircle(
            LisLat, LisLon, LisLat, LisLon, fraction: 0.5);

        Assert.Equal(LisLat, lat, precision: 6);
        Assert.Equal(LisLon, lon, precision: 6);
    }
}
