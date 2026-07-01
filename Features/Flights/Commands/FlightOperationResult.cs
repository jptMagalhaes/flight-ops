using FlightOps.Enums;

namespace FlightOps.Features.Flights.Commands;

public sealed class FlightOperationResult<T>
{
    public T? Value { get; init; }
    public FlightCommandsError Error { get; init; }

    public bool IsSuccess => Error == FlightCommandsError.None;

    public static FlightOperationResult<T> Success(T value) =>
        new() { Value = value, Error = FlightCommandsError.None };

    public static FlightOperationResult<T> Fail(FlightCommandsError error) =>
        new() { Error = error };
}
