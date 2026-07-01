namespace FlightOps.Models.Pages.Simulation;

public class ActiveFlightSimulationModel
{
    public int Id { get; set; }
    public string OriginIata { get; set; } = string.Empty;
    public string OriginName { get; set; } = string.Empty;
    public string DestinationIata { get; set; } = string.Empty;
    public string DestinationName { get; set; } = string.Empty;
    public double OriginLatitude { get; set; }
    public double OriginLongitude { get; set; }
    public double DestinationLatitude { get; set; }
    public double DestinationLongitude { get; set; }
    public string AircraftName { get; set; } = string.Empty;
    public string AircraftModel { get; set; } = string.Empty;
    public double DistanceKm { get; set; }
    public double TotalFuel { get; set; }
    public double TakeOffFuel { get; set; }
    public double FuelConsumed { get; set; }
    public double FuelBurnRatePerHour { get; set; }
    public double Progress { get; set; }
    public double CurrentLatitude { get; set; }
    public double CurrentLongitude { get; set; }
    public double CurrentAltitudeM { get; set; }
    public double RemainingSeconds { get; set; }
    public double ElapsedSeconds { get; set; }
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
}
