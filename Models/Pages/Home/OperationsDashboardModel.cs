namespace FlightOps.Models.Pages.Home;

public class OperationsDashboardModel
{
    public int ActiveFlights { get; set; }
    public int AircraftOnGround { get; set; }
    public int AircraftAirborne { get; set; }
    public int UpcomingDeparturesCount { get; set; }
    public int CancelledTodayCount { get; set; }
    public List<DashboardDepartureModel> NextDepartures { get; set; } = [];
    public List<DashboardFleetItemModel> FleetStatus { get; set; } = [];
}
