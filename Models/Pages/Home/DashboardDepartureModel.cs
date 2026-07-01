using FlightOps.Enums;

namespace FlightOps.Models.Pages.Home;

public class DashboardDepartureModel
{
    public int FlightId { get; set; }
    public DateTime DepartureTime { get; set; }
    public string OriginIata { get; set; } = "";
    public string DestinationIata { get; set; } = "";
    public string Registration { get; set; } = "";
    public FlightStatus Status { get; set; }
}
