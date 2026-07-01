using FlightOps.Entities;
using FlightOps.Enums;
using FlightOps.Features.Flights.Scheduling;
using FlightOps.Models.Pages.Simulation;
using FlightOps.Repositories.Flights;

namespace FlightOps.Features.Flights.Simulation;

public class FlightSimulator(
    IFlightRepository flightRepository,
    IFlightLifecycleApplier flightLifecycleService,
    TimeProvider timeProvider) : IFlightSimulator
{
    public async Task<IReadOnlyList<ActiveFlightSimulationModel>> GetActiveFlightsAsync(DateTime? at = null)
    {
        DateTime now = await ApplyTransitionsAndGetNowAsync(at);

        List<Flight> flights = await flightRepository.GetDepartedFlightsAsync();

        return flights
            .Where(f => now >= f.DepartureTime && now < f.ArrivalTime)
            .Select(f => BuildSnapshot(f, now))
            .OrderBy(s => s.RemainingSeconds)
            .ToList();
    }

    public async Task<ActiveFlightSimulationModel?> GetActiveFlightAsync(int flightId, DateTime? at = null)
    {
        DateTime now = await ApplyTransitionsAndGetNowAsync(at);

        Flight? flight = await flightRepository.GetFlightByIdAsync(flightId);
        if (flight is null || flight.Status != FlightStatus.Departed)
            return null;

        if (now < flight.DepartureTime || now >= flight.ArrivalTime)
            return null;

        return BuildSnapshot(flight, now);
    }

    public async Task<bool> CompleteFlightIfDueAsync(int flightId, DateTime? at = null)
    {
        DateTime now = await ApplyTransitionsAndGetNowAsync(at);

        Flight? flight = await flightRepository.GetFlightByIdAsync(flightId);
        return flight is not null && flight.Status == FlightStatus.Arrived;
    }

    private async Task<DateTime> ApplyTransitionsAndGetNowAsync(DateTime? at)
    {
        DateTime now = at ?? timeProvider.GetUtcNow().UtcDateTime;
        await flightLifecycleService.ApplyTransitionsAsync(now);
        return now;
    }

    private static ActiveFlightSimulationModel BuildSnapshot(Flight flight, DateTime now)
    {
        double totalSeconds = (flight.ArrivalTime - flight.DepartureTime).TotalSeconds;
        double elapsedSeconds = Math.Clamp((now - flight.DepartureTime).TotalSeconds, 0, totalSeconds);
        double progress = totalSeconds > 0 ? elapsedSeconds / totalSeconds : 1.0;
        double remainingSeconds = Math.Max(0, totalSeconds - elapsedSeconds);

        (double lat, double lon) = GeoInterpolationService.InterpolateGreatCircle(
            flight.Origin.Latitude, flight.Origin.Longitude,
            flight.Destination.Latitude, flight.Destination.Longitude,
            progress);

        double cruiseFuelPerHour = flight.Aircraft.CruiseSpeedKmh * flight.Aircraft.FuelConsumptionPerKm;
        double distanceTraveled = flight.Distance * progress;
        double fuelConsumed = flight.Aircraft.TakeOffEffort + distanceTraveled * flight.Aircraft.FuelConsumptionPerKm;

        return new ActiveFlightSimulationModel
        {
            Id = flight.Id,
            OriginIata = flight.Origin.IATA ?? string.Empty,
            OriginName = flight.Origin.Name ?? string.Empty,
            DestinationIata = flight.Destination.IATA ?? string.Empty,
            DestinationName = flight.Destination.Name ?? string.Empty,
            OriginLatitude = flight.Origin.Latitude,
            OriginLongitude = flight.Origin.Longitude,
            DestinationLatitude = flight.Destination.Latitude,
            DestinationLongitude = flight.Destination.Longitude,
            AircraftName = flight.Aircraft.Name ?? string.Empty,
            AircraftModel = flight.Aircraft.Model ?? string.Empty,
            DistanceKm = flight.Distance,
            TotalFuel = flight.Fuel,
            TakeOffFuel = flight.Aircraft.TakeOffEffort,
            FuelConsumed = Math.Min(fuelConsumed, flight.Fuel),
            FuelBurnRatePerHour = cruiseFuelPerHour,
            Progress = progress,
            CurrentLatitude = lat,
            CurrentLongitude = lon,
            CurrentAltitudeM = 8000 + Math.Sin(progress * Math.PI) * 2000,
            RemainingSeconds = remainingSeconds,
            ElapsedSeconds = elapsedSeconds,
            DepartureTime = flight.DepartureTime,
            ArrivalTime = flight.ArrivalTime
        };
    }
}
