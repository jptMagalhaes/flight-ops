using FlightOps.Entities;
using FlightOps.Mappers;
using FlightOps.Models.Pages.Airports;
using FlightOps.Repositories.Airports;
using FlightOps.Repositories.Flights;
using FlightOps.Repositories.Aircrafts;

namespace FlightOps.Features.Airports.Queries;

public sealed class AirportDetailsQuery(
    IAirportRepository airportRepository,
    IAircraftRepository aircraftRepository,
    IFlightRepository flightRepository) : IAirportDetailsQuery
{
    public async Task<AirportDetailModel?> BuildAsync(int airportId)
    {
        Airport? airport = await airportRepository.GetAirportByIdAsync(airportId);
        if (airport is null)
            return null;

        List<Aircraft> aircrafts = await aircraftRepository.GetAircraftAtAirportAsync(airportId);
        List<Flight> flights = await flightRepository.GetFlightsByAirportIdAsync(airportId);

        return new AirportDetailModel
        {
            Airport = airport.ToModel(),
            AircraftOnGround = aircrafts.ToModels(),
            Flights = flights.ToModels()
        };
    }
}
