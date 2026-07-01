using System.Globalization;

namespace FlightOps.Helpers;

public static class NumberFormatHelper
{
    public static string Invariant(double value, string format = "F2") =>
        value.ToString(format, CultureInfo.InvariantCulture);
}
