using FlightOps.Entities;
using FlightOps.Enums;

namespace FlightOps.Features.Flights.Scheduling;

public interface IFlightScheduleValidator
{
    Task<FlightCommandsError?> ValidateAsync(Flight candidate, int? excludeFlightId, bool validateOrigin);
}
