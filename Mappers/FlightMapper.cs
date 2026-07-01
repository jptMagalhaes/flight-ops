using FlightOps.Entities;
using FlightOps.Models.Forms;

namespace FlightOps.Mappers;

public static class FlightMapper
{
    public static FlightModel ToModel(this Flight flight) => new()
    {
        Id = flight.Id,
        Distance = flight.Distance,
        Fuel = flight.Fuel,
        DepartureTime = flight.DepartureTime,
        ArrivalTime = flight.ArrivalTime,
        Status = flight.Status,
        OriginId = flight.OriginId,
        DestinationId = flight.DestinationId,
        AircraftId = flight.AircraftId,
        Origin = flight.Origin.ToModel(),
        Destination = flight.Destination.ToModel(),
        Aircraft = flight.Aircraft.ToModel()
    };

    public static Flight ToEntity(this FlightModel model) => new()
    {
        Id = model.Id,
        OriginId = model.OriginId,
        DestinationId = model.DestinationId,
        AircraftId = model.AircraftId,
        DepartureTime = model.DepartureTime,
        Status = model.Status
    };

    public static List<FlightModel> ToModels(this List<Flight> flights) =>
        [.. flights.Select(ToModel)];
}
