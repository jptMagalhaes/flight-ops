using FlightOps.Entities;
using FlightOps.Infrastructure;
using FlightOps.Mappers;
using FlightOps.Models.Forms;
using FlightOps.Models.Pages.Aircraft;
using FlightOps.Features.Aircrafts.Queries;
using FlightOps.Helpers;
using FlightOps.Repositories.Airports;
using FlightOps.Repositories.Aircrafts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using FlightOps.Resources;

namespace FlightOps.Controllers;

[Authorize(Policy = "ViewerOrOperator")]
public class AircraftController(
    ILogger<AircraftController> logger,
    IAircraftRepository aircraftRepository,
    IAircraftDetailsQuery aircraftDetailsQuery,
    IAirportRepository airportRepository,
    IStringLocalizer<SharedResources> localizer) : Controller
{
    public async Task<IActionResult> Index()
    {
        List<Aircraft> aircrafts = await aircraftRepository.GetAllAircraftAsync();
        return View(aircrafts.ToModels());
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        AircraftDetailModel? detail = await aircraftDetailsQuery.BuildAsync(id);
        if (detail is null)
            return NotFound();

        return View(detail);
    }

    [Authorize(Policy = "OperatorOnly")]
    public async Task<IActionResult> Create()
    {
        AircraftModel model = new();
        await PopulateAirportsAsync(model);
        return View(model);
    }

    [HttpGet]
    [Authorize(Policy = "OperatorOnly")]
    public async Task<IActionResult> Edit(int id)
    {
        Aircraft? aircraft = await aircraftRepository.GetAircraftByIdAsync(id);
        if (aircraft is null)
            return NotFound();

        AircraftModel model = aircraft.ToModel();
        await PopulateAirportsAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "OperatorOnly")]
    public async Task<IActionResult> Create(AircraftModel aircraft)
    {
        if (!ModelState.IsValid)
        {
            await PopulateAirportsAsync(aircraft);
            return View(aircraft);
        }

        return await SaveAircraftAsync(
            aircraft,
            () => aircraftRepository.CreateAircraftAsync(aircraft.ToEntity()),
            "Error.CreateAircraftFailed",
            "Aircraft created: {Aircraft}");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "OperatorOnly")]
    public async Task<IActionResult> Edit(int id, AircraftModel aircraft)
    {
        if (id != aircraft.Id)
            return NotFound();

        if (!ModelState.IsValid)
        {
            await PopulateAirportsAsync(aircraft);
            return View(aircraft);
        }

        Aircraft? existing = await aircraftRepository.GetAircraftByIdAsync(id);
        if (existing is null)
            return NotFound();

        Aircraft entity = aircraft.ToEntity();
        if (existing.CurrentAirportId is null)
        {
            entity.CurrentAirportId = null;
            entity.HangarBay = existing.HangarBay;
        }

        return await SaveAircraftAsync(
            aircraft,
            () => aircraftRepository.UpdateAircraftAsync(entity),
            "Error.UpdateAircraftFailed",
            "Aircraft updated: {Aircraft}");
    }

    private async Task<IActionResult> SaveAircraftAsync(
        AircraftModel model,
        Func<Task<Aircraft?>> persist,
        string failureMessageKey,
        string successLogTemplate)
    {
        try
        {
            Aircraft? saved = await persist();
            if (saved is null)
            {
                ModelState.AddModelError(string.Empty, localizer[failureMessageKey]);
                await PopulateAirportsAsync(model);
                return View(model);
            }

            logger.LogInformation(successLogTemplate, saved.ToModel());
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException ex) when (DbUniqueViolationDetector.IsUniqueViolation(ex, "Registration"))
        {
            ModelState.AddModelError(nameof(model.Registration), localizer["Error.DuplicateAircraftRegistration"]);
            await PopulateAirportsAsync(model);
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
            Aircraft? deleted = await aircraftRepository.DeleteAircraftAsync(id);
            if (deleted is null)
                return NotFound();

            logger.LogInformation("Aircraft deleted: {Aircraft}", deleted.ToModel());
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = localizer["Error.DeleteAircraftReferenced"].Value;
            return RedirectToAction(nameof(Index));
        }
    }

    private async Task PopulateAirportsAsync(AircraftModel model)
    {
        List<Airport> airports = await airportRepository.GetAllAirportsAsync();
        model.AirportOptions = AirportSelectListHelper.ToSelectList(airports, model.CurrentAirportId);
    }
}
