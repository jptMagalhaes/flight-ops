namespace FlightOps.Enums;

public enum FlightCommandsError
{
    None,
    NotFound,
    InvalidRoute,
    MissingReferences,
    AircraftScheduleConflict,
    AircraftWrongOrigin,
    InvalidStatusTransition
}
