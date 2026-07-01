using FlightOps.Entities;
using FlightOps.Enums;
using FlightOps.Repositories.Flights;

namespace FlightOps.Features.Flights.Scheduling;

public sealed class FlightScheduleValidator(
    IFlightRepository flightRepository,
    IAircraftLocationResolver aircraftLocationResolver) : IFlightScheduleValidator
{
    public async Task<FlightCommandsError?> ValidateAsync(
        Flight candidate,
        int? excludeFlightId,
        bool validateOrigin)
    {
        if (candidate.Status is not (FlightStatus.Scheduled or FlightStatus.Departed))
            return null;

        List<Flight> blocking = await flightRepository.GetBlockingFlightsForAircraftAsync(
            candidate.AircraftId,
            excludeFlightId);

        foreach (Flight other in blocking)
        {
            if (IntervalsOverlap(
                    candidate.DepartureTime, candidate.ArrivalTime,
                    other.DepartureTime, other.ArrivalTime))
                return FlightCommandsError.AircraftScheduleConflict;
        }

        if (!validateOrigin)
            return null;

        int? expectedOrigin = await aircraftLocationResolver.ResolveAirportIdAsync(
            candidate.AircraftId,
            candidate.DepartureTime);

        if (expectedOrigin is not null && candidate.OriginId != expectedOrigin.Value)
            return FlightCommandsError.AircraftWrongOrigin;

        return null;
    }

    internal static bool IntervalsOverlap(
        DateTime departureA,
        DateTime arrivalA,
        DateTime departureB,
        DateTime arrivalB) =>
        departureA < arrivalB && departureB < arrivalA;
}
