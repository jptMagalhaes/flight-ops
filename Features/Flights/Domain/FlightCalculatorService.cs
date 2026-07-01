namespace FlightOps.Features.Flights.Domain;

public static class FlightCalculatorService
{
    private const double EarthRadiusKm = 6371.0;

    public static double CalculateDistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        double lat1Rad = DegreesToRadians(lat1);
        double lat2Rad = DegreesToRadians(lat2);
        double deltaLat = DegreesToRadians(lat2 - lat1);
        double deltaLon = DegreesToRadians(lon2 - lon1);

        double a = Math.Pow(Math.Sin(deltaLat / 2), 2)
            + Math.Cos(lat1Rad) * Math.Cos(lat2Rad) * Math.Pow(Math.Sin(deltaLon / 2), 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return EarthRadiusKm * c;
    }

    public static double CalculateFuel(double distanceKm, double fuelConsumptionPerKm, int takeoffEffort) =>
        distanceKm * fuelConsumptionPerKm + takeoffEffort;

    public static TimeSpan CalculateFlightDuration(double distanceKm, double cruiseSpeedKmh) =>
        TimeSpan.FromHours(distanceKm / cruiseSpeedKmh);

    public static FlightMetrics CalculateFlightMetrics(
        double originLat, double originLon,
        double destinationLat, double destinationLon,
        double fuelConsumptionPerKm, int takeoffEffort, double cruiseSpeedKmh,
        DateTime departureTime)
    {
        double distance = CalculateDistanceKm(originLat, originLon, destinationLat, destinationLon);
        double fuel = CalculateFuel(distance, fuelConsumptionPerKm, takeoffEffort);
        DateTime arrivalTime = departureTime + CalculateFlightDuration(distance, cruiseSpeedKmh);

        return new FlightMetrics(distance, fuel, arrivalTime);
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180.0;
}

public readonly record struct FlightMetrics(double Distance, double Fuel, DateTime ArrivalTime);
