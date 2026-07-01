using FlightOps.Entities;
using FlightOps.Repositories.Flights;
using FlightOps.Repositories.Aircrafts;

namespace FlightOps.Features.Flights.Scheduling;

public sealed class AircraftLocationResolver(
    IFlightRepository flightRepository,
    IAircraftRepository aircraftRepository) : IAircraftLocationResolver
{
    public async Task<int?> ResolveAirportIdAsync(int aircraftId, DateTime at)
    {
        Flight? lastArrived = await flightRepository.GetLastArrivedBeforeAsync(aircraftId, at);
        if (lastArrived is not null)
            return lastArrived.DestinationId;

        Aircraft? aircraft = await aircraftRepository.GetAircraftByIdAsync(aircraftId);
        return aircraft?.CurrentAirportId;
    }

    public async Task<IReadOnlyList<Aircraft>> GetAvailableAtAirportAsync(int airportId, DateTime at)
    {
        List<Aircraft> fleet = await aircraftRepository.GetAllAircraftAsync();
        List<Aircraft> available = [];

        foreach (Aircraft aircraft in fleet)
        {
            int? location = await ResolveAirportIdAsync(aircraft.Id, at);
            if (location == airportId)
                available.Add(aircraft);
        }

        return available;
    }
}
