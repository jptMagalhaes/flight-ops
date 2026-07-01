using FlightOps.Infrastructure;

namespace FlightOps.Tests.Unit;

public class FlightTimeConverterTests
{
    private readonly IFlightTimeConverter _converter = new FlightTimeConverter();

    [Theory]
    [InlineData(-60)]
    [InlineData(0)]
    [InlineData(300)]
    public void ToUtc_AndBack_RoundTripsWallClock(int offsetMinutes)
    {
        DateTime localWallClock = new(2026, 7, 1, 14, 35, 0, DateTimeKind.Unspecified);

        DateTime utc = _converter.ToUtc(localWallClock, offsetMinutes);
        DateTime roundTrip = _converter.ToLocalWallClock(utc, offsetMinutes);

        Assert.Equal(DateTimeKind.Utc, utc.Kind);
        Assert.Equal(localWallClock, roundTrip);
        Assert.Equal(DateTimeKind.Unspecified, roundTrip.Kind);
    }

    [Fact]
    public void GetUtcDayRange_WithNegativeOffset_CoversExpectedUtcWindow()
    {
        DateTime utcNow = new(2026, 7, 1, 23, 30, 0, DateTimeKind.Utc);

        (DateTime startUtc, DateTime endUtc) = _converter.GetUtcDayRange(-60, utcNow);

        Assert.Equal(new DateTime(2026, 7, 1, 23, 0, 0, DateTimeKind.Utc), startUtc);
        Assert.Equal(new DateTime(2026, 7, 2, 23, 0, 0, DateTimeKind.Utc), endUtc);
    }

    [Fact]
    public void GetUtcDayRange_WithPositiveOffset_CoversExpectedUtcWindow()
    {
        DateTime utcNow = new(2026, 7, 1, 1, 30, 0, DateTimeKind.Utc);

        (DateTime startUtc, DateTime endUtc) = _converter.GetUtcDayRange(300, utcNow);

        Assert.Equal(new DateTime(2026, 6, 30, 5, 0, 0, DateTimeKind.Utc), startUtc);
        Assert.Equal(new DateTime(2026, 7, 1, 5, 0, 0, DateTimeKind.Utc), endUtc);
    }
}
