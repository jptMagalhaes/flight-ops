using FlightOps.Entities;

namespace FlightOps.Features.Flights.Scheduling;

public interface IAircraftLocationResolver
{
    Task<int?> ResolveAirportIdAsync(int aircraftId, DateTime at);
    Task<IReadOnlyList<Aircraft>> GetAvailableAtAirportAsync(int airportId, DateTime at);
}
