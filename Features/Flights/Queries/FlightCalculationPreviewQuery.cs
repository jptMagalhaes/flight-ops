using FlightOps.Entities;
using FlightOps.Models.Pages.Flights;
using FlightOps.Repositories.Aircrafts;
using FlightOps.Repositories.Airports;
using FlightOps.Features.Flights.Domain;

namespace FlightOps.Features.Flights.Queries;

public sealed class FlightCalculationPreviewQuery(
    IAirportRepository airportRepository,
    IAircraftRepository aircraftRepository) : IFlightCalculationPreviewQuery
{
    public async Task<FlightCalculationPreviewModel?> BuildAsync(
        int originId,
        int destinationId,
        int aircraftId,
        DateTime departureTime)
    {
        if (originId <= 0 || destinationId <= 0 || aircraftId <= 0 || originId == destinationId)
            return null;

        Airport? origin = await airportRepository.GetAirportByIdAsync(originId);
        Airport? destination = await airportRepository.GetAirportByIdAsync(destinationId);
        Aircraft? aircraft = await aircraftRepository.GetAircraftByIdAsync(aircraftId);

        if (origin is null || destination is null || aircraft is null || aircraft.CruiseSpeedKmh <= 0)
            return null;

        FlightMetrics metrics = FlightCalculatorService.CalculateFlightMetrics(
            origin.Latitude, origin.Longitude,
            destination.Latitude, destination.Longitude,
            aircraft.FuelConsumptionPerKm, aircraft.TakeOffEffort, aircraft.CruiseSpeedKmh,
            departureTime);

        return new FlightCalculationPreviewModel
        {
            Distance = metrics.Distance,
            Fuel = metrics.Fuel,
            ArrivalTime = metrics.ArrivalTime
        };
    }
}
