using FlightOps.Models.Forms;

namespace FlightOps.Models.Pages.Flights;

public class FlightReportRowModel
{
    public required FlightModel Flight { get; init; }
    public TimeSpan Duration { get; init; }
    public double AverageSpeedKmh { get; init; }
}
