namespace FlightOps.Features.Flights.Scheduling;

public interface IFlightLifecycleApplier
{
    Task<bool> ApplyTransitionsAsync(DateTime? at = null);
}
