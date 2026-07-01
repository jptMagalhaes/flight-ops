using FlightOps.Enums;

namespace FlightOps.Entities
{
    public class Flight
    {
        public int Id { get; set; }
        public double Distance { get; set; }
        public double Fuel { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public FlightStatus Status { get; set; }
        public int AircraftId { get; set; }
        public Aircraft Aircraft { get; set; } = null!;
        public int DestinationId { get; set; }
        public Airport Destination { get; set; } = null!;
        public int OriginId { get; set; }
        public Airport Origin { get; set; } = null!;
    }
}
