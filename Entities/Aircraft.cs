namespace FlightOps.Entities
{
    public class Aircraft
    {
        public int Id { get; set; }
        public int TakeOffEffort { get; set; }
        public double FuelConsumptionPerKm { get; set; }
        public double CruiseSpeedKmh { get; set; }
        public string? Name { get; set; }
        public string? Model { get; set; }
        public string? Registration { get; set; }
        public string? HangarBay { get; set; }
        public int? CurrentAirportId { get; set; }
        public Airport? CurrentAirport { get; set; }
    }
}
