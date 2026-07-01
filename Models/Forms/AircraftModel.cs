using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FlightOps.Models.Forms;

public class AircraftModel
{
    public int Id { get; set; }

    [Required, Display(Name = "Registration")]
    public string? Registration { get; set; }

    [Required, Display(Name = "Name")]
    public string? Name { get; set; }

    [Required, Display(Name = "Model")]
    public string? Model { get; set; }

    [Display(Name = "HangarBay")]
    public string? HangarBay { get; set; }

    [Required, Display(Name = "HomeAirport")]
    public int CurrentAirportId { get; set; }

    [Required, Range(1, int.MaxValue), Display(Name = "TakeOffEffort")]
    public int TakeOffEffort { get; set; }

    [Required, Range(0.01, double.MaxValue), Display(Name = "FuelConsumptionPerKm")]
    public double FuelConsumptionPerKm { get; set; }

    [Required, Range(0.01, double.MaxValue), Display(Name = "CruiseSpeedKmh")]
    public double CruiseSpeedKmh { get; set; }

    public string? CurrentAirportIata { get; set; }
    public string? CurrentAirportName { get; set; }
    public bool IsInFlight { get; set; }

    public IEnumerable<SelectListItem> AirportOptions { get; set; } = [];
}
