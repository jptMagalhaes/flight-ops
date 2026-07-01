using FlightOps.Models.Forms;

namespace FlightOps.Models.Pages.Airports;

public class AirportDetailModel
{
    public AirportModel Airport { get; set; } = null!;
    public IReadOnlyList<AircraftModel> AircraftOnGround { get; set; } = [];
    public IReadOnlyList<FlightModel> Flights { get; set; } = [];
}
