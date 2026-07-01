using FlightOps.Entities;
using FlightOps.Models.Forms;

namespace FlightOps.Mappers;

public static class AirportMapper
{
    public static AirportModel ToModel(this Airport airport) => new()
    {
        Id = airport.Id,
        Name = airport.Name,
        City = airport.City,
        Country = airport.Country,
        IATA = airport.IATA,
        Latitude = airport.Latitude,
        Longitude = airport.Longitude
    };

    public static Airport ToEntity(this AirportModel airport) => new()
    {
        Id = airport.Id,
        Name = airport.Name,
        City = airport.City,
        Country = airport.Country,
        IATA = airport.IATA,
        Latitude = airport.Latitude,
        Longitude = airport.Longitude
    };

    public static List<AirportModel> ToModels(this List<Airport> airports) =>
        [.. airports.Select(ToModel)];

    public static List<Airport> ToEntities(this List<AirportModel> airports) =>
        [.. airports.Select(ToEntity)];
}
