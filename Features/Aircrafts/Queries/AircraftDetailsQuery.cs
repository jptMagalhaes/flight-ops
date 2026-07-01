using FlightOps.Entities;
using FlightOps.Mappers;
using FlightOps.Models.Pages.Aircraft;
using FlightOps.Repositories.Flights;
using FlightOps.Repositories.Aircrafts;

namespace FlightOps.Features.Aircrafts.Queries;

public sealed class AircraftDetailsQuery(
    IAircraftRepository aircraftRepository,
    IFlightRepository flightRepository) : IAircraftDetailsQuery
{
    public async Task<AircraftDetailModel?> BuildAsync(int aircraftId)
    {
        Aircraft? aircraft = await aircraftRepository.GetAircraftByIdAsync(aircraftId);
        if (aircraft is null)
            return null;

        List<Flight> flights = await flightRepository.GetFlightsByAircraftIdAsync(aircraftId);

        return new AircraftDetailModel
        {
            Aircraft = aircraft.ToModel(),
            Flights = flights.ToModels()
        };
    }
}
