using System.ComponentModel.DataAnnotations;
using FlightOps.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace FlightOps.Models.Forms;

public class FlightModel
{
    public int Id { get; set; }
    public double Distance { get; set; }
    public double Fuel { get; set; }

    [Required]
    [Display(Name = "DepartureTime")]
    public DateTime DepartureTime { get; set; }

    [Display(Name = "ArrivalTime")]
    public DateTime ArrivalTime { get; set; }

    [Display(Name = "Status")]
    public FlightStatus Status { get; set; }

    [Required, Display(Name = "Origin")]
    public int OriginId { get; set; }

    [Required, Display(Name = "Destination")]
    public int DestinationId { get; set; }

    [Required, Display(Name = "Aircraft")]
    public int AircraftId { get; set; }

    [ValidateNever]
    public AircraftModel Aircraft { get; set; } = null!;

    [ValidateNever]
    public AirportModel Destination { get; set; } = null!;

    [ValidateNever]
    public AirportModel Origin { get; set; } = null!;
}
