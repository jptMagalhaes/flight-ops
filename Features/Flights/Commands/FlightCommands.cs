using FlightOps.Entities;
using FlightOps.Enums;
using FlightOps.Features.Flights.Scheduling;
using FlightOps.Infrastructure;
using FlightOps.Repositories.Airports;
using FlightOps.Repositories.Flights;
using FlightOps.Repositories.Aircrafts;
using FlightOps.Features.Flights.Domain;

namespace FlightOps.Features.Flights.Commands;

public class FlightCommands(
    ILogger<FlightCommands> logger,
    IFlightRepository flightRepository,
    IAirportRepository airportRepository,
    IFlightLifecycleApplier flightLifecycleService,
    IFlightScheduleValidator flightScheduleValidator,
    IAircraftRepository aircraftRepository,
    TimeProvider timeProvider) : IFlightCommands
{
    public async Task<List<Flight>> GetAllFlightsAsync()
        => await flightRepository.GetAllFlightsAsync();

    public async Task<Flight?> GetFlightByIdAsync(int id)
        => await flightRepository.GetFlightByIdAsync(id);

    public async Task<FlightOperationResult<Flight>> CreateFlightAsync(Flight flight)
    {
        await flightLifecycleService.ApplyTransitionsAsync();

        if (flight.DestinationId == flight.OriginId)
            return Fail(FlightCommandsError.InvalidRoute, flight.AircraftId);

        ApplyManualDepartRules(null, flight);

        if (!await TryRecalculateFlight(flight))
            return Fail(FlightCommandsError.MissingReferences, flight.AircraftId);

        using IDisposable bookingLock = await AircraftBookingLock.AcquireAsync(flight.AircraftId);

        FlightCommandsError? validationError = await flightScheduleValidator.ValidateAsync(
            flight,
            excludeFlightId: null,
            validateOrigin: flight.Status == FlightStatus.Scheduled);

        if (validationError is not null)
            return Fail(validationError.Value, flight.AircraftId);

        Flight? created = await flightRepository.CreateFlightAsync(flight);
        if (created is null)
            return Fail(FlightCommandsError.MissingReferences, flight.AircraftId);

        await aircraftRepository.SyncHangarLocationsAsync(timeProvider.GetUtcNow().UtcDateTime);
        return FlightOperationResult<Flight>.Success(created);
    }

    public async Task<FlightOperationResult<Flight>> UpdateFlightAsync(Flight flight)
    {
        await flightLifecycleService.ApplyTransitionsAsync();

        Flight? existingFlight = await flightRepository.GetFlightByIdAsync(flight.Id);
        if (existingFlight is null)
            return Fail(FlightCommandsError.NotFound, flight.Id);

        if (existingFlight.Status is FlightStatus.Arrived or FlightStatus.Cancelled)
            return Fail(FlightCommandsError.InvalidStatusTransition, flight.Id);

        if (!IsStatusTransitionAllowed(existingFlight.Status, flight.Status))
            return Fail(FlightCommandsError.InvalidStatusTransition, flight.Id);

        if (existingFlight.Status == FlightStatus.Departed
            && (flight.OriginId != existingFlight.OriginId
                || flight.AircraftId != existingFlight.AircraftId
                || flight.Status == FlightStatus.Scheduled))
            return Fail(FlightCommandsError.InvalidStatusTransition, flight.Id);

        bool isDeparted = existingFlight.Status != FlightStatus.Scheduled;
        Flight toUpdate = new()
        {
            Id = existingFlight.Id,
            OriginId = isDeparted ? existingFlight.OriginId : flight.OriginId,
            DestinationId = flight.DestinationId,
            AircraftId = isDeparted ? existingFlight.AircraftId : flight.AircraftId,
            DepartureTime = flight.DepartureTime,
            Status = flight.Status,
        };

        ApplyManualDepartRules(existingFlight, toUpdate);

        bool validateOrigin = existingFlight.Status == FlightStatus.Scheduled
            && toUpdate.Status is FlightStatus.Scheduled or FlightStatus.Departed;

        if (!await TryRecalculateFlight(toUpdate))
            return Fail(FlightCommandsError.MissingReferences, toUpdate.Id);

        using IDisposable bookingLock = await AircraftBookingLock.AcquireAsync(toUpdate.AircraftId);

        FlightCommandsError? validationError = await flightScheduleValidator.ValidateAsync(
            toUpdate,
            excludeFlightId: toUpdate.Id,
            validateOrigin: validateOrigin);

        if (validationError is not null)
            return Fail(validationError.Value, toUpdate.Id);

        Flight? updated = await flightRepository.UpdateFlightAsync(toUpdate);
        if (updated is null)
            return Fail(FlightCommandsError.NotFound, toUpdate.Id);

        await aircraftRepository.SyncHangarLocationsAsync(timeProvider.GetUtcNow().UtcDateTime);
        return FlightOperationResult<Flight>.Success(updated);
    }

    public async Task<Flight?> DeleteFlightAsync(int id)
    {
        Flight? existing = await flightRepository.GetFlightByIdAsync(id);
        if (existing is null)
            return null;

        using IDisposable bookingLock = await AircraftBookingLock.AcquireAsync(existing.AircraftId);
        return await flightRepository.DeleteFlightAsync(id);
    }

    private FlightOperationResult<Flight> Fail(FlightCommandsError error, int id)
    {
        logger.LogWarning("Flight operation rejected: {Error} (Id: {Id})", error, id);
        return FlightOperationResult<Flight>.Fail(error);
    }

    private void ApplyManualDepartRules(Flight? existing, Flight candidate)
    {
        bool isNewDepart = existing is null
            ? candidate.Status == FlightStatus.Departed
            : existing.Status == FlightStatus.Scheduled && candidate.Status == FlightStatus.Departed;

        if (isNewDepart)
            candidate.DepartureTime = timeProvider.GetUtcNow().UtcDateTime;
    }

    private static bool IsStatusTransitionAllowed(FlightStatus from, FlightStatus to)
    {
        if (from == to)
            return true;

        return from switch
        {
            FlightStatus.Scheduled => to is FlightStatus.Departed or FlightStatus.Cancelled,
            FlightStatus.Departed => false,
            _ => false
        };
    }

    private async Task<bool> TryRecalculateFlight(Flight flight)
    {
        Airport? origin = await airportRepository.GetAirportByIdAsync(flight.OriginId);
        Airport? destination = await airportRepository.GetAirportByIdAsync(flight.DestinationId);
        Aircraft? aircraft = await aircraftRepository.GetAircraftByIdAsync(flight.AircraftId);
        if (origin is null || destination is null || aircraft is null)
            return false;

        FlightMetrics metrics = FlightCalculatorService.CalculateFlightMetrics(
            origin.Latitude, origin.Longitude,
            destination.Latitude, destination.Longitude,
            aircraft.FuelConsumptionPerKm, aircraft.TakeOffEffort, aircraft.CruiseSpeedKmh,
            flight.DepartureTime);

        flight.Distance = metrics.Distance;
        flight.Fuel = metrics.Fuel;
        flight.ArrivalTime = metrics.ArrivalTime;

        return true;
    }
}
