using FlightOps.Entities;
using FlightOps.Enums;
using FlightOps.Features.Flights.Scheduling;
using FlightOps.Features.Flights.Simulation;
using FlightOps.Models.Pages.Simulation;
using FlightOps.Repositories.Flights;
using Moq;

namespace FlightOps.Tests.Unit;

public class FlightSimulatorTests
{
    private static readonly DateTime Departure = new(2026, 1, 1, 10, 0, 0);
    private static readonly DateTime Arrival = new(2026, 1, 1, 11, 0, 0);

    private static Flight DepartedFlight(int id = 1) => new()
    {
        Id = id,
        Status = FlightStatus.Departed,
        DepartureTime = Departure,
        ArrivalTime = Arrival,
        Distance = 277,
        Fuel = 1031,
        Origin = new Airport { IATA = "LIS", Name = "Lisbon", Latitude = 38.7742, Longitude = -9.1342 },
        Destination = new Airport { IATA = "OPO", Name = "Porto", Latitude = 41.2481, Longitude = -8.6814 },
        Aircraft = new Aircraft
        {
            Name = "Test",
            Model = "A320",
            TakeOffEffort = 200,
            FuelConsumptionPerKm = 3,
            CruiseSpeedKmh = 800
        }
    };

    [Fact]
    public async Task GetActiveFlightsAsync_MidFlight_ReturnsSnapshotWithHalfProgress()
    {
        DateTime now = Departure.AddMinutes(30);
        Mock<IFlightRepository> flightRepository = new();
        flightRepository.Setup(r => r.GetDepartedFlightsAsync()).ReturnsAsync([DepartedFlight()]);
        Mock<IFlightLifecycleApplier> lifecycleApplier = new();

        FlightSimulator simulator = new(flightRepository.Object, lifecycleApplier.Object, TimeProvider.System);

        IReadOnlyList<ActiveFlightSimulationModel> active = await simulator.GetActiveFlightsAsync(now);

        Assert.Single(active);
        ActiveFlightSimulationModel snapshot = active[0];
        Assert.InRange(snapshot.Progress, 0.45, 0.55);
        Assert.InRange(snapshot.RemainingSeconds, 1700, 1900);
        Assert.True(snapshot.FuelConsumed > snapshot.TakeOffFuel);
    }

    [Fact]
    public async Task GetActiveFlightsAsync_BeforeDeparture_ExcludesFlight()
    {
        Mock<IFlightRepository> flightRepository = new();
        flightRepository.Setup(r => r.GetDepartedFlightsAsync()).ReturnsAsync([DepartedFlight()]);
        Mock<IFlightLifecycleApplier> lifecycleApplier = new();

        FlightSimulator simulator = new(flightRepository.Object, lifecycleApplier.Object, TimeProvider.System);

        IReadOnlyList<ActiveFlightSimulationModel> active =
            await simulator.GetActiveFlightsAsync(Departure.AddMinutes(-1));

        Assert.Empty(active);
    }

    [Fact]
    public async Task GetActiveFlightsAsync_AtOrAfterArrival_ExcludesFlight()
    {
        Mock<IFlightRepository> flightRepository = new();
        flightRepository.Setup(r => r.GetDepartedFlightsAsync()).ReturnsAsync([DepartedFlight()]);
        Mock<IFlightLifecycleApplier> lifecycleApplier = new();

        FlightSimulator simulator = new(flightRepository.Object, lifecycleApplier.Object, TimeProvider.System);

        IReadOnlyList<ActiveFlightSimulationModel> atArrival =
            await simulator.GetActiveFlightsAsync(Arrival);
        IReadOnlyList<ActiveFlightSimulationModel> afterArrival =
            await simulator.GetActiveFlightsAsync(Arrival.AddMinutes(1));

        Assert.Empty(atArrival);
        Assert.Empty(afterArrival);
    }

    [Fact]
    public async Task GetActiveFlightAsync_ScheduledStatus_ReturnsNull()
    {
        Flight scheduled = DepartedFlight();
        scheduled.Status = FlightStatus.Scheduled;

        Mock<IFlightRepository> flightRepository = new();
        flightRepository.Setup(r => r.GetFlightByIdAsync(1)).ReturnsAsync(scheduled);
        Mock<IFlightLifecycleApplier> lifecycleApplier = new();

        FlightSimulator simulator = new(flightRepository.Object, lifecycleApplier.Object, TimeProvider.System);

        ActiveFlightSimulationModel? snapshot =
            await simulator.GetActiveFlightAsync(1, Departure.AddMinutes(30));

        Assert.Null(snapshot);
    }

    [Fact]
    public async Task CompleteFlightIfDueAsync_AfterLifecycle_ReturnsTrueWhenArrived()
    {
        Flight arrived = DepartedFlight();
        arrived.Status = FlightStatus.Arrived;

        Mock<IFlightRepository> flightRepository = new();
        flightRepository.Setup(r => r.GetFlightByIdAsync(1)).ReturnsAsync(arrived);
        Mock<IFlightLifecycleApplier> lifecycleApplier = new();

        FlightSimulator simulator = new(flightRepository.Object, lifecycleApplier.Object, TimeProvider.System);

        bool completed = await simulator.CompleteFlightIfDueAsync(1, Arrival);

        Assert.True(completed);
        lifecycleApplier.Verify(s => s.ApplyTransitionsAsync(Arrival), Times.Once);
    }
}
