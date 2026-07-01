using FlightOps.Repositories.Flights;
using FlightOps.Repositories.Aircrafts;

namespace FlightOps.Features.Flights.Scheduling;

public sealed class FlightLifecycleApplier(
    IFlightRepository flightRepository,
    IAircraftRepository aircraftRepository,
    TimeProvider timeProvider) : IFlightLifecycleApplier
{
    public async Task<bool> ApplyTransitionsAsync(DateTime? at = null)
    {
        DateTime now = at ?? timeProvider.GetUtcNow().UtcDateTime;
        int transitions = await flightRepository.ApplyLifecycleTransitionsAsync(now);
        if (transitions > 0)
            await aircraftRepository.SyncHangarLocationsAsync(now);

        return transitions > 0;
    }
}
