using FlightOps.Entities;
using FlightOps.Features.Flights.Scheduling;

namespace FlightOps.Features.Flights.Commands;

public interface IFlightCommands
{
    Task<List<Flight>> GetAllFlightsAsync();
    Task<Flight?> GetFlightByIdAsync(int id);
    Task<FlightOperationResult<Flight>> CreateFlightAsync(Flight flight);
    Task<FlightOperationResult<Flight>> UpdateFlightAsync(Flight flight);
    Task<Flight?> DeleteFlightAsync(int id);
}
