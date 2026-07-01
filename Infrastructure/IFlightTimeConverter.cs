namespace FlightOps.Infrastructure;

public interface IFlightTimeConverter
{
    DateTime ToUtc(DateTime localWallClock, int offsetMinutes);
    DateTime ToLocalWallClock(DateTime utc, int offsetMinutes);
    (DateTime StartUtc, DateTime EndUtc) GetUtcDayRange(int offsetMinutes, DateTime utcReference);
}
