using FlightOps.Entities;
using FlightOps.Models.Forms;

namespace FlightOps.Mappers;

public static class AircraftMapper
{
    public static AircraftModel ToModel(this Aircraft aircraft) => new()
    {
        Id = aircraft.Id,
        Registration = aircraft.Registration,
        Name = aircraft.Name,
        Model = aircraft.Model,
        HangarBay = aircraft.HangarBay,
        CurrentAirportId = aircraft.CurrentAirportId ?? 0,
        TakeOffEffort = aircraft.TakeOffEffort,
        FuelConsumptionPerKm = aircraft.FuelConsumptionPerKm,
        CruiseSpeedKmh = aircraft.CruiseSpeedKmh,
        CurrentAirportIata = aircraft.CurrentAirport?.IATA,
        CurrentAirportName = aircraft.CurrentAirport?.Name,
        IsInFlight = aircraft.CurrentAirportId is null
    };

    public static Aircraft ToEntity(this AircraftModel aircraft) => new()
    {
        Id = aircraft.Id,
        Registration = aircraft.Registration,
        Name = aircraft.Name,
        Model = aircraft.Model,
        HangarBay = aircraft.HangarBay,
        CurrentAirportId = aircraft.CurrentAirportId > 0 ? aircraft.CurrentAirportId : null,
        TakeOffEffort = aircraft.TakeOffEffort,
        FuelConsumptionPerKm = aircraft.FuelConsumptionPerKm,
        CruiseSpeedKmh = aircraft.CruiseSpeedKmh
    };

    public static List<AircraftModel> ToModels(this List<Aircraft> aircrafts) =>
        [.. aircrafts.Select(ToModel)];
}
