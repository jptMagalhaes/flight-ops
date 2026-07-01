using FlightOps.Entities;
using FlightOps.Infrastructure;
using FlightOps.Mappers;
using FlightOps.Models.Forms;
using FlightOps.Models.Pages.Airports;
using FlightOps.Features.Airports.Queries;
using FlightOps.Repositories.Airports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using FlightOps.Resources;

namespace FlightOps.Controllers;

[Authorize(Policy = "ViewerOrOperator")]
public class AirportController(
    ILogger<AirportController> logger,
    IAirportRepository airportRepository,
    IAirportDetailsQuery airportDetailsQuery,
    IStringLocalizer<SharedResources> localizer) : Controller
{
    public async Task<IActionResult> Index()
    {
        List<Airport> airports = await airportRepository.GetAllAirportsAsync();
        return View(airports.ToModels());
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        AirportDetailModel? detail = await airportDetailsQuery.BuildAsync(id);
        if (detail is null)
            return NotFound();

        return View(detail);
    }

    [Authorize(Policy = "OperatorOnly")]
    public IActionResult Create() => View(new AirportModel());

    [HttpGet]
    [Authorize(Policy = "OperatorOnly")]
    public async Task<IActionResult> Edit(int id)
    {
        Airport? airportEntity = await airportRepository.GetAirportByIdAsync(id);
        if (airportEntity is null)
            return NotFound();
        return View(airportEntity.ToModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "OperatorOnly")]
    public async Task<IActionResult> Create(AirportModel airport)
    {
        if (!ModelState.IsValid)
            return View(airport);

        return await SaveAirportAsync(
            airport,
            () => airportRepository.CreateAirportAsync(airport.ToEntity()),
            "Error.CreateAirportFailed",
            "Airport created successfully: {Airport}");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "OperatorOnly")]
    public async Task<IActionResult> Edit(int id, AirportModel airport)
    {
        if (id != airport.Id)
            return NotFound();

        if (!ModelState.IsValid)
            return View(airport);

        return await SaveAirportAsync(
            airport,
            () => airportRepository.UpdateAirportAsync(airport.ToEntity()),
            "Error.UpdateAirportFailed",
            "Airport updated successfully: {Airport}");
    }

    private async Task<IActionResult> SaveAirportAsync(
        AirportModel model,
        Func<Task<Airport?>> persist,
        string failureMessageKey,
        string successLogTemplate)
    {
        try
        {
            Airport? saved = await persist();
            if (saved is null)
            {
                ModelState.AddModelError(string.Empty, localizer[failureMessageKey]);
                return View(model);
            }

            logger.LogInformation(successLogTemplate, saved.ToModel());
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException ex) when (DbUniqueViolationDetector.IsUniqueViolation(ex, "IATA"))
        {
            ModelState.AddModelError(nameof(model.IATA), localizer["Error.DuplicateAirportIata"]);
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "OperatorOnly")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            Airport? airportEntity = await airportRepository.DeleteAirportAsync(id);
            if (airportEntity is null)
                return NotFound();

            logger.LogInformation("Airport deleted successfully: {Airport}", airportEntity.ToModel());
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = localizer["Error.DeleteAirportReferenced"].Value;
            return RedirectToAction(nameof(Index));
        }
    }
}
