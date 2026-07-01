using FlightOps.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

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
        }
    }
}