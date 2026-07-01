using System.ComponentModel.DataAnnotations;

namespace FlightOps.Models.Forms;

public class AirportModel
{
    public int Id { get; set; }

    [Required, Display(Name = "Name")]
    public string? Name { get; set; }

    [Required, Display(Name = "City")]
    public string? City { get; set; }

    [Required, Display(Name = "Country")]
    public string? Country { get; set; }

    [Required, StringLength(3, MinimumLength = 3), Display(Name = "IATA")]
    public string? IATA { get; set; }

    [Required, Display(Name = "Latitude")]
    public double Latitude { get; set; }

    [Required, Display(Name = "Longitude")]
    public double Longitude { get; set; }
}
