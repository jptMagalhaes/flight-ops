using FlightOps.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FlightOps.Data
{
    public class FlightOpsDbContext(DbContextOptions<FlightOpsDbContext> options)
        : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Airport> Airports => Set<Airport>();
        public DbSet<Aircraft> Aircrafts => Set<Aircraft>();
        public DbSet<Flight> Flights => Set<Flight>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Flight>()
                .HasOne(f => f.Aircraft)
                .WithMany()
                .HasForeignKey(f => f.AircraftId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Flight>()
                .HasOne(f => f.Destination)
                .WithMany()
                .HasForeignKey(f => f.DestinationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Flight>()
                .HasOne(f => f.Origin)
                .WithMany()
                .HasForeignKey(f => f.OriginId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Aircraft>(entity =>
            {
                entity.HasOne(p => p.CurrentAirport)
                    .WithMany()
                    .HasForeignKey(p => p.CurrentAirportId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasIndex(p => p.Registration).IsUnique();
            });

            modelBuilder.Entity<Airport>()
                .HasIndex(a => a.IATA)
                .IsUnique();

            // SQLite has no native datetime type — EF Core stores DateTime as ISO-8601 text and,
            // on read, returns it with Kind=Unspecified, discarding the fact that every DateTime
            // in this app is UTC (produced via TimeProvider). System.Text.Json only emits the "Z"
            // suffix for Kind=Utc, so without this, JSON responses (e.g. the Simulation API) send
            // timestamps with no timezone marker — which browsers parse as *local* time, not UTC.
            // That mismatch was causing the Simulation page's client-side progress calculation to
            // disagree with the server about whether a flight had arrived, retrying
            // /Simulation/CompleteFlight in a tight loop once the client (wrongly) thought a
            // flight was done before the server did. Restoring Kind=Utc on every read fixes the
            // serialization for this and any other DateTime-returning endpoint.
            ValueConverter<DateTime, DateTime> utcConverter = new(
                toProvider => toProvider,
                fromProvider => DateTime.SpecifyKind(fromProvider, DateTimeKind.Utc));
            ValueConverter<DateTime?, DateTime?> utcNullableConverter = new(
                toProvider => toProvider,
                fromProvider => fromProvider.HasValue ? DateTime.SpecifyKind(fromProvider.Value, DateTimeKind.Utc) : fromProvider);

            foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (IMutableProperty property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime))
                        property.SetValueConverter(utcConverter);
                    else if (property.ClrType == typeof(DateTime?))
                        property.SetValueConverter(utcNullableConverter);
                }
            }
        }
    }
}