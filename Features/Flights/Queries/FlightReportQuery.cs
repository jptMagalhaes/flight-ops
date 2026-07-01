using FlightOps.Entities;
using FlightOps.Mappers;
using FlightOps.Models.Pages.Flights;
using FlightOps.Repositories.Flights;

namespace FlightOps.Features.Flights.Queries;

public sealed class FlightReportQuery(
    IFlightRepository flightRepository) : IFlightReportQuery
{
    public async Task<FlightReportDetailModel?> BuildAsync(int flightId)
    {
        Flight? flight = await flightRepository.GetFlightByIdAsync(flightId);
        if (flight is null)
            return null;

        TimeSpan duration = flight.ArrivalTime - flight.DepartureTime;
        double fuelPer100Km = flight.Distance > 0 ? flight.Fuel / flight.Distance * 100 : 0;
        double hours = duration.TotalHours;
        double avgSpeed = hours > 0 ? flight.Distance / hours : 0;

        return new FlightReportDetailModel
        {
            Flight = flight.ToModel(),
            Duration = duration,
            FuelPer100Km = fuelPer100Km,
            AverageSpeedKmh = avgSpeed
        };
    }
}
