using FlightOps.Data;
using FlightOps.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlightOps.Repositories.Airports;

public class AirportRepository(FlightOpsDbContext context) : IAirportRepository
{
    public async Task<List<Airport>> GetAllAirportsAsync() =>
        await context.Airports.AsNoTracking().ToListAsync();

    public async Task<Airport?> GetAirportByIdAsync(int id) =>
        await context.Airports.AsNoTracking().SingleOrDefaultAsync(a => a.Id == id);

    public async Task<Airport?> CreateAirportAsync(Airport airport)
    {
        await context.Airports.AddAsync(airport);
        await context.SaveChangesAsync();
        return airport;
    }

    public async Task<Airport?> UpdateAirportAsync(Airport airport)
    {
        context.Airports.Update(airport);
        await context.SaveChangesAsync();
        return airport;
    }

    public async Task<Airport?> DeleteAirportAsync(int id)
    {
        Airport? airport = await GetAirportByIdAsync(id);
        if (airport is null)
            return null;

        context.Airports.Remove(airport);
        await context.SaveChangesAsync();
        return airport;
    }
}
