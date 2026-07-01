using FlightOps.Models.Forms;

namespace FlightOps.Models.Pages.Aircraft;

public class AircraftDetailModel
{
    public AircraftModel Aircraft { get; set; } = null!;
    public IReadOnlyList<FlightModel> Flights { get; set; } = [];
}
