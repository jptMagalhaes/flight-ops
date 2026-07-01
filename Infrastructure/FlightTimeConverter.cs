namespace FlightOps.Infrastructure;

public sealed class FlightTimeConverter : IFlightTimeConverter
{
    public DateTime ToUtc(DateTime localWallClock, int offsetMinutes)
    {
        DateTime local = DateTime.SpecifyKind(localWallClock, DateTimeKind.Unspecified);
        DateTime utc = local.AddMinutes(offsetMinutes);
        return DateTime.SpecifyKind(utc, DateTimeKind.Utc);
    }

    public DateTime ToLocalWallClock(DateTime utc, int offsetMinutes)
    {
        DateTime utcValue = NormalizeUtc(utc);
        DateTime local = utcValue.AddMinutes(-offsetMinutes);
        return DateTime.SpecifyKind(local, DateTimeKind.Unspecified);
    }

    public (DateTime StartUtc, DateTime EndUtc) GetUtcDayRange(int offsetMinutes, DateTime utcReference)
    {
        DateTime utcNow = NormalizeUtc(utcReference);
        DateTime localNow = utcNow.AddMinutes(-offsetMinutes);
        DateTime localStart = DateTime.SpecifyKind(localNow.Date, DateTimeKind.Unspecified);
        DateTime startUtc = ToUtc(localStart, offsetMinutes);
        return (startUtc, startUtc.AddDays(1));
    }

    private static DateTime NormalizeUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}
