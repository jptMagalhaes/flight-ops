using FlightOps.Entities;

namespace FlightOps.Repositories.Airports;

public interface IAirportRepository
{
    Task<List<Airport>> GetAllAirportsAsync();
    Task<Airport?> GetAirportByIdAsync(int id);
    Task<Airport?> CreateAirportAsync(Airport airport);
    Task<Airport?> UpdateAirportAsync(Airport airport);
    Task<Airport?> DeleteAirportAsync(int id);
}
