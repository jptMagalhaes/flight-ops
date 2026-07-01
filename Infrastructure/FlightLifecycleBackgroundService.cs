using FlightOps.Features.Flights.Scheduling;

namespace FlightOps.Infrastructure;

/// <summary>
/// Drives flight status transitions on a timer so they happen even when nobody is hitting the
/// app. The commands/queries that already call ApplyTransitionsAsync per-request stay in place
/// as defense in depth — this just guarantees it also runs without traffic.
/// </summary>
public sealed class FlightLifecycleBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<FlightLifecycleBackgroundService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(Interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                IFlightLifecycleApplier applier = scope.ServiceProvider.GetRequiredService<IFlightLifecycleApplier>();
                await applier.ApplyTransitionsAsync();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Background flight lifecycle transition failed");
            }
        }
    }
}
