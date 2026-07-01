using FlightOps.Models.Pages.Simulation;

namespace FlightOps.Features.Flights.Simulation;

public interface IFlightSimulator
{
    Task<IReadOnlyList<ActiveFlightSimulationModel>> GetActiveFlightsAsync(DateTime? at = null);
    Task<ActiveFlightSimulationModel?> GetActiveFlightAsync(int flightId, DateTime? at = null);
    Task<bool> CompleteFlightIfDueAsync(int flightId, DateTime? at = null);
}
