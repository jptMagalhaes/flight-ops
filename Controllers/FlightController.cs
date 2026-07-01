using FlightOps.Entities;
using FlightOps.Enums;
using FlightOps.Helpers;
using FlightOps.Mappers;
using FlightOps.Models.Forms;
using FlightOps.Models.Pages.Flights;
using FlightOps.Features.Flights.Commands;
using FlightOps.Features.Flights.Queries;
using FlightOps.Features.Flights.Scheduling;
using FlightOps.Infrastructure;
using FlightOps.Repositories.Airports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using FlightOps.Repositories.Aircrafts;
using FlightOps.Resources;

namespace FlightOps.Controllers;

[Authorize(Policy = "ViewerOrOperator")]
public class FlightController(
    ILogger<FlightController> logger,
    IFlightCommands flightService,
    IFlightReportQuery flightReportQuery,
    IFlightCalculationPreviewQuery flightCalculationPreviewQuery,
    IAirportRepository airportRepository,
    IAircraftRepository aircraftRepository,
    IAircraftLocationResolver aircraftLocationResolver,
    IStringLocalizer<SharedResources> localizer,
    IFlightTimeConverter flightTimeConverter,
    IUserTimeZoneAccessor userTimeZoneAccessor,
    TimeProvider timeProvider) : Controller
{
    public async Task<IActionResult> Index()
    {
        List<Flight> flights = await flightService.GetAllFlightsAsync();
        return View(flights.ToModels());
    }

    public async Task<IActionResult> Report()
    {
        List<Flight> flights = await flightService.GetAllFlightsAsync();
        return View(flights.ToModels().ToReportRows());
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        Flight? flight = await flightService.GetFlightByIdAsync(id);
        if (flight is null)
            return NotFound();

        return View(flight.ToModel());
    }

    [HttpGet]
    public async Task<IActionResult> ReportDetail(int id)
    {
        FlightReportDetailModel? detail = await flightReportQuery.BuildAsync(id);
        if (detail is null)
            return NotFound();

        return View(detail);
    }

    [HttpGet]
    public async Task<IActionResult> CalculatePreview(
        int originId,
        int destinationId,
        int aircraftId,
        DateTime departureTime)
    {
        DateTime utcDeparture = NormalizeUtc(departureTime);
        FlightCalculationPreviewModel? preview = await flightCalculationPreviewQuery.BuildAsync(
            originId, destinationId, aircraftId, utcDeparture);

        if (preview is null)
            return BadRequest();

        return Json(preview);
    }

    [Authorize(Policy = "OperatorOnly")]
    public async Task<IActionResult> Create()
    {
        FlightModel model = new()
        {
            Status = FlightStatus.Scheduled,
            DepartureTime = ToLocalWallClock(timeProvider.GetUtcNow().UtcDateTime)
        };
        await PopulateDropdownsAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "OperatorOnly")]
    public async Task<IActionResult> Create(FlightModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(model);
            return View(model);
        }

        model.DepartureTime = ToUtc(model.DepartureTime);
        FlightOperationResult<Flight> result = await flightService.CreateFlightAsync(model.ToEntity());
        if (!result.IsSuccess)
        {
            await AddFlightErrorAsync(result.Error, model, isCreate: true);
            model.DepartureTime = ToLocalWallClock(model.DepartureTime);
            await PopulateDropdownsAsync(model);
            return View(model);
        }

        logger.LogInformation("Flight created: {FlightId}", result.Value!.Id);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Policy = "OperatorOnly")]
    public async Task<IActionResult> Edit(int id)
    {
        Flight? flight = await flightService.GetFlightByIdAsync(id);
        if (flight is null)
            return NotFound();

        FlightModel model = flight.ToModel();
        model.DepartureTime = ToLocalWallClock(model.DepartureTime);
        await PopulateDropdownsAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "OperatorOnly")]
    public async Task<IActionResult> Edit(int id, FlightModel model)
    {
        if (id != model.Id)
            return NotFound();

        if (!ModelState.IsValid)
            return await ReturnEditViewAsync(model);

        model.DepartureTime = ToUtc(model.DepartureTime);
        FlightOperationResult<Flight> result = await flightService.UpdateFlightAsync(model.ToEntity());
        if (!result.IsSuccess)
        {
            await AddFlightErrorAsync(result.Error, model, isCreate: false);
            model.DepartureTime = ToLocalWallClock(model.DepartureTime);

            Flight? existing = await flightService.GetFlightByIdAsync(id);
            if (existing is not null)
                model.Status = existing.Status;

            return await ReturnEditViewAsync(model);
        }

        logger.LogInformation("Flight updated: {FlightId}", result.Value!.Id);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "OperatorOnly")]
    public async Task<IActionResult> Delete(int id)
    {
        Flight? deleted = await flightService.DeleteFlightAsync(id);
        if (deleted is null)
            return NotFound();

        logger.LogInformation("Flight deleted: {FlightId}", deleted.Id);
        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult> ReturnEditViewAsync(FlightModel model)
    {
        (List<Airport> airports, List<Aircraft> aircrafts) = await PopulateDropdownsAsync(model);
        HydrateNavigationModels(model, airports, aircrafts);
        ViewBag.EditBlocked = model.Status is FlightStatus.Arrived or FlightStatus.Cancelled;
        return View(model);
    }

    private async Task AddFlightErrorAsync(FlightCommandsError error, FlightModel model, bool isCreate)
    {
        if (error == FlightCommandsError.AircraftWrongOrigin)
        {
            IReadOnlyList<Aircraft> available = await aircraftLocationResolver.GetAvailableAtAirportAsync(
                model.OriginId,
                model.DepartureTime);

            string labels = string.Join(", ", available.Select(FormatAircraftLabel));

            string message = available.Count > 0
                ? localizer["Error.AircraftWrongOrigin.WithAvailable", labels]
                : localizer["Error.AircraftWrongOrigin.NoneAvailable"];

            ModelState.AddModelError(string.Empty, message);
            return;
        }

        string key = error switch
        {
            FlightCommandsError.AircraftScheduleConflict => "Error.AircraftScheduleConflict",
            FlightCommandsError.InvalidStatusTransition => "Error.InvalidStatusTransition",
            _ => isCreate ? "Error.CreateFlightFailed" : "Error.UpdateFlightFailed"
        };

        ModelState.AddModelError(string.Empty, localizer[key]);
    }

    private static string FormatAircraftLabel(Aircraft aircraft)
    {
        if (!string.IsNullOrWhiteSpace(aircraft.Name) && !string.IsNullOrWhiteSpace(aircraft.Registration))
            return $"{aircraft.Name} ({aircraft.Registration})";

        return aircraft.Name ?? aircraft.Registration ?? $"#{aircraft.Id}";
    }

    private async Task<(List<Airport> airports, List<Aircraft> aircrafts)> PopulateDropdownsAsync(FlightModel model)
    {
        List<Airport> airports = await airportRepository.GetAllAirportsAsync();
        List<Aircraft> aircrafts = await aircraftRepository.GetAllAircraftAsync();

        ViewBag.OriginId = AirportSelectListHelper.ToSelectList(airports, model.OriginId);
        ViewBag.DestinationId = AirportSelectListHelper.ToSelectList(airports, model.DestinationId);
        ViewBag.AircraftId = new SelectList(aircrafts, "Id", "Name", model.AircraftId);

        IEnumerable<FlightStatus> statuses = Enum.GetValues<FlightStatus>();
        if (model.Status == FlightStatus.Departed)
            statuses = statuses.Where(s => s != FlightStatus.Scheduled);

        ViewBag.Status = FlightStatusHelper.ToSelectList(statuses, model.Status, localizer);
        return (airports, aircrafts);
    }

    private static void HydrateNavigationModels(
        FlightModel model,
        IReadOnlyList<Airport> airports,
        IReadOnlyList<Aircraft> aircrafts)
    {
        if (airports.FirstOrDefault(a => a.Id == model.OriginId) is { } origin)
            model.Origin = origin.ToModel();

        if (airports.FirstOrDefault(a => a.Id == model.DestinationId) is { } destination)
            model.Destination = destination.ToModel();

        if (aircrafts.FirstOrDefault(p => p.Id == model.AircraftId) is { } aircraft)
            model.Aircraft = aircraft.ToModel();
    }

    private DateTime ToUtc(DateTime localWallClock)
    {
        int offsetMinutes = userTimeZoneAccessor.GetOffsetMinutes();
        return flightTimeConverter.ToUtc(localWallClock, offsetMinutes);
    }

    private DateTime ToLocalWallClock(DateTime utc)
    {
        int offsetMinutes = userTimeZoneAccessor.GetOffsetMinutes();
        return flightTimeConverter.ToLocalWallClock(utc, offsetMinutes);
    }

    private static DateTime NormalizeUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}
