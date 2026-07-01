using FlightOps.Entities;

namespace FlightOps.Repositories.Aircrafts;

public interface IAircraftRepository
{
    Task<List<Aircraft>> GetAllAircraftAsync();
    Task<Aircraft?> GetAircraftByIdAsync(int id);
    Task<Aircraft?> CreateAircraftAsync(Aircraft aircraft);
    Task<Aircraft?> UpdateAircraftAsync(Aircraft aircraft);
    Task<Aircraft?> DeleteAircraftAsync(int id);
    Task SyncHangarLocationsAsync(DateTime now);
    Task<List<Aircraft>> GetAircraftAtAirportAsync(int airportId);
}
