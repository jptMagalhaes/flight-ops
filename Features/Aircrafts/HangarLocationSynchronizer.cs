using FlightOps.Data;
using FlightOps.Entities;
using FlightOps.Enums;
using Microsoft.EntityFrameworkCore;

namespace FlightOps.Features.Aircrafts;

public static class HangarLocationSynchronizer
{
    public static async Task SyncAsync(FlightOpsDbContext context, DateTime now)
    {
        List<int> inFlightIds = await context.Flights
            .Where(f => f.Status == FlightStatus.Departed
                && f.DepartureTime <= now
                && now < f.ArrivalTime)
            .Select(f => f.AircraftId)
            .Distinct()
            .ToListAsync();

        await context.Aircrafts
            .Where(p => inFlightIds.Contains(p.Id) && p.CurrentAirportId != null)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(p => p.CurrentAirportId, (int?)null));

        List<int> onGroundIds = await context.Aircrafts
            .Where(p => !inFlightIds.Contains(p.Id))
            .Select(p => p.Id)
            .ToListAsync();

        foreach (int aircraftId in onGroundIds)
        {
            Flight? lastArrived = await context.Flights.AsNoTracking()
                .Where(f => f.AircraftId == aircraftId
                    && f.Status == FlightStatus.Arrived
                    && f.ArrivalTime <= now)
                .OrderByDescending(f => f.ArrivalTime)
                .FirstOrDefaultAsync();

            if (lastArrived is null)
                continue;

            await context.Aircrafts
                .Where(p => p.Id == aircraftId && p.CurrentAirportId != lastArrived.DestinationId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(p => p.CurrentAirportId, lastArrived.DestinationId));
        }
    }
}
