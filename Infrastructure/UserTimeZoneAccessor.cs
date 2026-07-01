using System.Globalization;

namespace FlightOps.Infrastructure;

public sealed class UserTimeZoneAccessor(IHttpContextAccessor httpContextAccessor) : IUserTimeZoneAccessor
{
    private const string CookieName = "fo_tz_offset";
    private const int MinOffsetMinutes = -14 * 60;
    private const int MaxOffsetMinutes = 14 * 60;

    public int GetOffsetMinutes()
    {
        HttpContext? context = httpContextAccessor.HttpContext;
        if (context is null)
            return 0;

        if (!context.Request.Cookies.TryGetValue(CookieName, out string? value))
            return 0;

        bool isValidOffset = int.TryParse(
            value,
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out int offsetMinutes);

        if (!isValidOffset || offsetMinutes < MinOffsetMinutes || offsetMinutes > MaxOffsetMinutes)
            return 0;

        return offsetMinutes;
    }
}
