using FlightOps.Entities;
using FlightOps.Features.Flights.Commands;
using FlightOps.Features.Flights.Simulation;
using FlightOps.Mappers;
using FlightOps.Models.Forms;
using FlightOps.Models.Pages.Simulation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlightOps.Controllers.Api;

[ApiController]
[Route("api/flights")]
[Authorize(Policy = "ViewerOrOperator")]
public class FlightsApiController(
    IFlightCommands flightCommands,
    IFlightSimulator flightSimulator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FlightModel>>> GetAll()
    {
        List<Flight> flights = await flightCommands.GetAllFlightsAsync();
        return Ok(flights.ToModels());
    }

    [HttpGet("active")]
    public async Task<ActionResult<IReadOnlyList<ActiveFlightSimulationModel>>> GetActive()
    {
        IReadOnlyList<ActiveFlightSimulationModel> active = await flightSimulator.GetActiveFlightsAsync();
        return Ok(active);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FlightModel>> GetById(int id)
    {
        Flight? flight = await flightCommands.GetFlightByIdAsync(id);
        if (flight is null)
            return NotFound();

        return Ok(flight.ToModel());
    }
}
