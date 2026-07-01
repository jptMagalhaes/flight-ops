using System.Collections.Concurrent;

namespace FlightOps.Infrastructure;

/// <summary>
/// Serializes flight create/update operations per aircraft so the validate-then-save
/// sequence in FlightCommands can't race across concurrent requests targeting the same aircraft.
/// </summary>
public static class AircraftBookingLock
{
    private static readonly ConcurrentDictionary<int, SemaphoreSlim> Locks = new();

    public static async Task<IDisposable> AcquireAsync(int aircraftId)
    {
        SemaphoreSlim semaphore = Locks.GetOrAdd(aircraftId, static _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync();
        return new Releaser(semaphore);
    }

    private sealed class Releaser(SemaphoreSlim semaphore) : IDisposable
    {
        public void Dispose() => semaphore.Release();
    }
}
