using FlightOps.Entities;

namespace FlightOps.Repositories.Flights;

public interface IFlightRepository
{
    Task<List<Flight>> GetAllFlightsAsync();
    Task<Flight?> GetFlightByIdAsync(int id);
    Task<Flight?> CreateFlightAsync(Flight flight);
    Task<Flight?> UpdateFlightAsync(Flight flight);
    Task<Flight?> DeleteFlightAsync(int id);
    Task<List<Flight>> GetDepartedFlightsAsync();
    Task<List<Flight>> GetBlockingFlightsForAircraftAsync(int aircraftId, int? excludeFlightId);
    Task<Flight?> GetLastArrivedBeforeAsync(int aircraftId, DateTime at);
    Task<int> ApplyLifecycleTransitionsAsync(DateTime now);
    Task<List<Flight>> GetFlightsByAircraftIdAsync(int aircraftId, int take = 20);
    Task<List<Flight>> GetFlightsByAirportIdAsync(int airportId, int take = 20);
    Task<FlightStatusCounts> GetDashboardCountsAsync(
        DateTime nowUtc,
        DateTime upcomingCutoffUtc,
        DateTime dayStartUtc,
        DateTime dayEndUtc);
    Task<List<Flight>> GetUpcomingScheduledFlightsAsync(DateTime now, int take);
}
