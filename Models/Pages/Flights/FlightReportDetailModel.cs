using FlightOps.Models.Forms;

namespace FlightOps.Models.Pages.Flights;

public class FlightReportDetailModel
{
    public FlightModel Flight { get; set; } = null!;
    public TimeSpan Duration { get; set; }
    public double FuelPer100Km { get; set; }
    public double AverageSpeedKmh { get; set; }
}
