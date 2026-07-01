using FlightOps.Data;
using FlightOps.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FlightOps.Tests.Support;

/// <summary>
/// Creates a FlightOpsDbContext backed by a real SQLite in-memory database (not EF Core's
/// InMemory provider, which does not support ExecuteUpdateAsync/ExecuteDeleteAsync). The
/// underlying connection must stay open for the lifetime of the context, or the in-memory
/// database is dropped.
/// </summary>
public sealed class TestDbContextFactory : IDisposable
{
    private readonly SqliteConnection _connection;
    public FlightOpsDbContext Context { get; }

    public TestDbContextFactory()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        DbContextOptions<FlightOpsDbContext> options = new DbContextOptionsBuilder<FlightOpsDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new FlightOpsDbContext(options);
        Context.Database.EnsureCreated();
    }

    public (Airport Origin, Airport Destination, Aircraft Aircraft) SeedMinimalFleet()
    {
        Airport origin = new() { IATA = "LIS", Name = "Humberto Delgado", City = "Lisbon", Country = "Portugal", Latitude = 38.7742, Longitude = -9.1342 };
        Airport destination = new() { IATA = "OPO", Name = "Francisco Sá Carneiro", City = "Porto", Country = "Portugal", Latitude = 41.2481, Longitude = -8.6814 };
        Aircraft aircraft = new() { Registration = "CS-TST", Name = "Test Aircraft", Model = "A320", TakeOffEffort = 200, FuelConsumptionPerKm = 3.0, CruiseSpeedKmh = 800, CurrentAirportId = null };

        Context.Airports.AddRange(origin, destination);
        Context.Aircrafts.Add(aircraft);
        Context.SaveChanges();

        return (origin, destination, aircraft);
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}
