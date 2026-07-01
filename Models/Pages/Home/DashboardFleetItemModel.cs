namespace FlightOps.Models.Pages.Home;

public class DashboardFleetItemModel
{
    public int AircraftId { get; set; }
    public string Registration { get; set; } = "";
    public string Model { get; set; } = "";
    public bool IsInFlight { get; set; }
    public string? LocationIata { get; set; }
}
