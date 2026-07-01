using FlightOps.Data.Seed;
using FlightOps.Entities;
using FlightOps.Enums;
using FlightOps.Features.Flights.Domain;
using FlightOps.Features.Aircrafts;
using Microsoft.EntityFrameworkCore;

namespace FlightOps.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(FlightOpsDbContext context, TimeProvider timeProvider)
    {
        if (await context.Airports.AnyAsync())
            return;

        Dictionary<string, Airport> airports = await SeedAirportsAsync(context);
        Dictionary<string, Aircraft> aircraft = await SeedAircraftAsync(context, airports);
        await SeedSampleFlightsAsync(context, airports, aircraft, timeProvider);
    }

    private static async Task<Dictionary<string, Airport>> SeedAirportsAsync(FlightOpsDbContext context)
    {
        List<Airport> airports = AirportSeedData.All
            .Select(record => new Airport
            {
                Name = record.Name,
                City = record.City,
                Country = record.Country,
                IATA = record.Iata,
                Latitude = record.Latitude,
                Longitude = record.Longitude
            })
            .ToList();

        context.Airports.AddRange(airports);
        await context.SaveChangesAsync();

        return airports.ToDictionary(a => a.IATA!, StringComparer.OrdinalIgnoreCase);
    }

    private static async Task<Dictionary<string, Aircraft>> SeedAircraftAsync(
        FlightOpsDbContext context,
        Dictionary<string, Airport> airports)
    {
        List<Aircraft> aircraft = AircraftSeedData.All
            .Select(record =>
            {
                Airport home = airports[record.HomeIata];
                return new Aircraft
                {
                    Registration = record.Registration,
                    Name = record.Name,
                    Model = record.Model,
                    HangarBay = record.HangarBay,
                    TakeOffEffort = record.TakeOffEffort,
                    FuelConsumptionPerKm = record.FuelConsumptionPerKm,
                    CruiseSpeedKmh = record.CruiseSpeedKmh,
                    CurrentAirportId = home.Id
                };
            })
            .ToList();

        context.Aircrafts.AddRange(aircraft);
        await context.SaveChangesAsync();

        return aircraft.ToDictionary(p => p.Registration!, StringComparer.OrdinalIgnoreCase);
    }

    private static async Task SeedSampleFlightsAsync(
        FlightOpsDbContext context,
        Dictionary<string, Airport> airports,
        Dictionary<string, Aircraft> aircraft,
        TimeProvider timeProvider)
    {
        DateTime now = timeProvider.GetUtcNow().UtcDateTime;

        context.Flights.AddRange(
            BuildFlight(airports["LIS"], airports["OPO"], aircraft["CS-TUI"], now.Date.AddHours(10), FlightStatus.Scheduled),
            BuildFlight(airports["LIS"], airports["MAD"], aircraft["CS-TUK"], now.AddMinutes(-45), FlightStatus.Departed),
            BuildFlight(airports["OPO"], airports["LHR"], aircraft["CS-TVB"], now.Date.AddDays(-1).AddHours(10), FlightStatus.Arrived),
            BuildFlight(airports["MAD"], airports["LIS"], aircraft["EC-MXY"], now.Date.AddHours(18), FlightStatus.Cancelled),
            BuildFlight(airports["FRA"], airports["AMS"], aircraft["D-AIZA"], now.Date.AddHours(14), FlightStatus.Scheduled),
            BuildFlight(airports["CDG"], airports["LHR"], aircraft["F-GKXY"], now.Date.AddHours(16), FlightStatus.Scheduled),
            BuildFlight(airports["DXB"], airports["SIN"], aircraft["A6-EOA"], now.AddMinutes(-120), FlightStatus.Departed),
            BuildFlight(airports["JFK"], airports["LAX"], aircraft["N12345"], now.Date.AddDays(-2).AddHours(8), FlightStatus.Arrived));

        await context.SaveChangesAsync();

        await HangarLocationSynchronizer.SyncAsync(context, now);
    }

    private static Flight BuildFlight(
        Airport origin,
        Airport destination,
        Aircraft aircraft,
        DateTime departureTime,
        FlightStatus status)
    {
        double distance = FlightCalculatorService.CalculateDistanceKm(
            origin.Latitude, origin.Longitude,
            destination.Latitude, destination.Longitude);
        TimeSpan duration = FlightCalculatorService.CalculateFlightDuration(
            distance, aircraft.CruiseSpeedKmh);

        return new Flight
        {
            OriginId = origin.Id,
            DestinationId = destination.Id,
            AircraftId = aircraft.Id,
            DepartureTime = departureTime,
            ArrivalTime = departureTime.Add(duration),
            Distance = distance,
            Fuel = FlightCalculatorService.CalculateFuel(
                distance, aircraft.FuelConsumptionPerKm, aircraft.TakeOffEffort),
            Status = status
        };
    }
}
