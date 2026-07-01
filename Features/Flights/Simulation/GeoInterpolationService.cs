namespace FlightOps.Features.Flights.Simulation;

public static class GeoInterpolationService
{
    public static (double Latitude, double Longitude) InterpolateGreatCircle(
        double lat1, double lon1, double lat2, double lon2, double fraction)
    {
        fraction = Math.Clamp(fraction, 0, 1);
        if (fraction <= 0) return (lat1, lon1);
        if (fraction >= 1) return (lat2, lon2);

        double lat1Rad = DegreesToRadians(lat1);
        double lon1Rad = DegreesToRadians(lon1);
        double lat2Rad = DegreesToRadians(lat2);
        double lon2Rad = DegreesToRadians(lon2);

        double deltaLon = lon2Rad - lon1Rad;
        double angularDistance = 2 * Math.Asin(Math.Sqrt(
            Math.Pow(Math.Sin((lat2Rad - lat1Rad) / 2), 2)
            + Math.Cos(lat1Rad) * Math.Cos(lat2Rad) * Math.Pow(Math.Sin(deltaLon / 2), 2)));

        if (angularDistance < 1e-10)
            return (lat1, lon1);

        double a = Math.Sin((1 - fraction) * angularDistance) / Math.Sin(angularDistance);
        double b = Math.Sin(fraction * angularDistance) / Math.Sin(angularDistance);

        double x = a * Math.Cos(lat1Rad) * Math.Cos(lon1Rad) + b * Math.Cos(lat2Rad) * Math.Cos(lon2Rad);
        double y = a * Math.Cos(lat1Rad) * Math.Sin(lon1Rad) + b * Math.Cos(lat2Rad) * Math.Sin(lon2Rad);
        double z = a * Math.Sin(lat1Rad) + b * Math.Sin(lat2Rad);

        double latRad = Math.Atan2(z, Math.Sqrt(x * x + y * y));
        double lonRad = Math.Atan2(y, x);

        return (RadiansToDegrees(latRad), RadiansToDegrees(lonRad));
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180.0;
    private static double RadiansToDegrees(double radians) => radians * 180.0 / Math.PI;
}
