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
            // Departed — currently in the air, so /Simulation has more than a couple of aircraft
            // moving on the globe the first time someone opens the app.
            BuildFlight(airports["LIS"], airports["OPO"], aircraft["CS-TUI"], now.AddMinutes(-8), FlightStatus.Departed),
            BuildFlight(airports["OPO"], airports["LHR"], aircraft["CS-TVB"], now.AddMinutes(-60), FlightStatus.Departed),
            BuildFlight(airports["MAD"], airports["BCN"], aircraft["EC-NXY"], now.AddMinutes(-10), FlightStatus.Departed),
            BuildFlight(airports["LHR"], airports["CDG"], aircraft["G-EUUP"], now.AddMinutes(-10), FlightStatus.Departed),
            BuildFlight(airports["FRA"], airports["MUC"], aircraft["D-AIZA"], now.AddMinutes(-8), FlightStatus.Departed),
            BuildFlight(airports["DXB"], airports["SIN"], aircraft["A6-EOA"], now.AddMinutes(-180), FlightStatus.Departed),
            BuildFlight(airports["JFK"], airports["LAX"], aircraft["N12345"], now.AddMinutes(-60), FlightStatus.Departed),
            BuildFlight(airports["SIN"], airports["HKG"], aircraft["9V-SMA"], now.AddMinutes(-50), FlightStatus.Departed),

            // Scheduled — later today or tomorrow, so the dashboard's "upcoming departures" and
            // the Create-flight origin/aircraft dropdowns have real near-term traffic to show.
            BuildFlight(airports["LIS"], airports["FAO"], aircraft["CS-TUK"], now.AddHours(3), FlightStatus.Scheduled),
            BuildFlight(airports["MAD"], airports["LIS"], aircraft["EC-MXY"], now.AddHours(5), FlightStatus.Scheduled),
            BuildFlight(airports["BCN"], airports["MAD"], aircraft["EC-MXZ"], now.AddHours(2), FlightStatus.Scheduled),
            BuildFlight(airports["LHR"], airports["DUB"], aircraft["G-EZAB"], now.AddHours(4), FlightStatus.Scheduled),
            BuildFlight(airports["CDG"], airports["BRU"], aircraft["F-GKXY"], now.AddHours(6), FlightStatus.Scheduled),
            BuildFlight(airports["ORY"], airports["LIS"], aircraft["F-HEPJ"], now.AddHours(30), FlightStatus.Scheduled),
            BuildFlight(airports["MUC"], airports["VIE"], aircraft["D-AISB"], now.AddHours(3), FlightStatus.Scheduled),
            BuildFlight(airports["AMS"], airports["FRA"], aircraft["PH-BXA"], now.AddHours(5), FlightStatus.Scheduled),
            BuildFlight(airports["BRU"], airports["CDG"], aircraft["OO-SNA"], now.AddHours(2), FlightStatus.Scheduled),
            BuildFlight(airports["ZRH"], airports["MUC"], aircraft["HB-JCA"], now.AddHours(4), FlightStatus.Scheduled),

            // Arrived — recent history, so aircraft/flight detail pages and the report have
            // completed sectors to display, not just a wall of "Scheduled".
            BuildFlight(airports["OPO"], airports["MAD"], aircraft["CS-TVC"], now.AddDays(-1).AddHours(9), FlightStatus.Arrived),
            BuildFlight(airports["VIE"], airports["PRG"], aircraft["OE-LBS"], now.AddDays(-1).AddHours(11), FlightStatus.Arrived),
            BuildFlight(airports["DUB"], airports["LHR"], aircraft["EI-DVM"], now.AddHours(-6), FlightStatus.Arrived),
            BuildFlight(airports["LAX"], airports["JFK"], aircraft["N837DN"], now.AddDays(-1).AddHours(7), FlightStatus.Arrived),
            BuildFlight(airports["DOH"], airports["DXB"], aircraft["A7-BEB"], now.AddHours(-8), FlightStatus.Arrived),
            BuildFlight(airports["HKG"], airports["SIN"], aircraft["B-HNL"], now.AddDays(-1).AddHours(14), FlightStatus.Arrived),

            // Cancelled — so the dashboard's "cancelled today" KPI and the report's status
            // filter both have a non-zero, non-trivial case to show.
            BuildFlight(airports["DEL"], airports["DXB"], aircraft["VT-ATV"], now.AddHours(7), FlightStatus.Cancelled),
            BuildFlight(airports["JNB"], airports["CAI"], aircraft["ZS-SNA"], now.AddHours(4), FlightStatus.Cancelled),
            BuildFlight(airports["SYD"], airports["MEL"], aircraft["VH-OQA"], now.AddHours(6), FlightStatus.Cancelled),
            BuildFlight(airports["CAI"], airports["JNB"], aircraft["SU-GDN"], now.AddHours(9), FlightStatus.Cancelled),

            // A couple of aircraft get a second, older sector so their detail pages show more
            // than one row of flight history.
            BuildFlight(airports["FAO"], airports["LIS"], aircraft["CS-TUI"], now.AddDays(-3).AddHours(8), FlightStatus.Arrived),
            BuildFlight(airports["LIS"], airports["MAD"], aircraft["EC-MXY"], now.AddDays(-2).AddHours(10), FlightStatus.Arrived),
            BuildFlight(airports["CDG"], airports["LHR"], aircraft["G-EUUP"], now.AddDays(-2).AddHours(15), FlightStatus.Arrived));

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
