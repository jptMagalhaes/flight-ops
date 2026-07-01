using FlightOps.Models.Forms;
using FlightOps.Models.Pages.Flights;

namespace FlightOps.Mappers;

public static class FlightReportRowMapper
{
    public static FlightReportRowModel ToReportRow(this FlightModel flight)
    {
        TimeSpan duration = flight.ArrivalTime - flight.DepartureTime;
        double hours = duration.TotalHours;
        double avgSpeed = hours > 0 ? flight.Distance / hours : 0;

        return new FlightReportRowModel
        {
            Flight = flight,
            Duration = duration,
            AverageSpeedKmh = avgSpeed
        };
    }

    public static List<FlightReportRowModel> ToReportRows(this IEnumerable<FlightModel> flights) =>
        [.. flights.Select(ToReportRow)];
}
