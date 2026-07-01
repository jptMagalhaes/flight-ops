using FlightOps.Models.Pages.Simulation;
using FlightOps.Features.Flights.Simulation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlightOps.Controllers;

[Authorize(Policy = "ViewerOrOperator")]
public class SimulationController(IFlightSimulator simulationService) : Controller
{
    public IActionResult Index() => View();

    [HttpGet]
    public async Task<IActionResult> GetActiveFlights()
    {
        IReadOnlyList<ActiveFlightSimulationModel> flights = await simulationService.GetActiveFlightsAsync();
        return Json(flights);
    }

    [HttpGet]
    public async Task<IActionResult> GetFlight(int id)
    {
        ActiveFlightSimulationModel? flight = await simulationService.GetActiveFlightAsync(id);
        if (flight is null)
            return NotFound();

        return Json(flight);
    }

    [HttpPost]
    [Authorize(Policy = "OperatorOnly")]
    public async Task<IActionResult> CompleteFlight(int id)
    {
        bool completed = await simulationService.CompleteFlightIfDueAsync(id);
        return completed ? Ok() : NotFound();
    }
}
