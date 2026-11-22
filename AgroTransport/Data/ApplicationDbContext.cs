using System.Threading.Tasks;
using AgroTransport.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AgroTransport.Data
{
    /// <summary>
    /// Entity Framework Core Database Context for AgroTransport.
    /// Manages ASP.NET Core Identity authentication tables and business domain models.
    /// Demonstrates Unit IV & V (Code First approach, Relationships, Indexing, and Views).
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Vehicle> Vehicles { get; set; } = null!;
        public DbSet<TransportTrip> TransportTrips { get; set; } = null!;
        public DbSet<Booking> Bookings { get; set; } = null!;
        public DbSet<TransportRequest> TransportRequests { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;

        // Database Views (Keyless Entity Types)
        public DbSet<VwActiveTrip> VwActiveTrips { get; set; } = null!;
        public DbSet<VwBookingSummary> VwBookingSummaries { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Unique Index on Vehicle Registration Number
            builder.Entity<Vehicle>()
                .HasIndex(v => v.RegistrationNumber)
                .IsUnique();

            // Vehicle Relationships
            builder.Entity<Vehicle>()
                .HasOne(v => v.Transporter)
                .WithMany(u => u.Vehicles)
                .HasForeignKey(v => v.TransporterId)
                .OnDelete(DeleteBehavior.Restrict);

            // TransportTrip Relationships
            builder.Entity<TransportTrip>()
                .HasOne(t => t.Transporter)
                .WithMany(u => u.Trips)
                .HasForeignKey(t => t.TransporterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TransportTrip>()
                .HasOne(t => t.Vehicle)
                .WithMany(v => v.Trips)
                .HasForeignKey(t => t.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Booking Relationships
            builder.Entity<Booking>()
                .HasOne(b => b.Trip)
                .WithMany(t => t.Bookings)
                .HasForeignKey(b => b.TripId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Booking>()
                .HasOne(b => b.Farmer)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.FarmerId)
                .OnDelete(DeleteBehavior.Restrict);

            // TransportRequest Relationships
            builder.Entity<TransportRequest>()
                .HasOne(r => r.Farmer)
                .WithMany(u => u.TransportRequests)
                .HasForeignKey(r => r.FarmerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Notification Relationships
            builder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Keyless Entity Mapping for SQL Views (Unit V)
            builder.Entity<VwActiveTrip>(eb =>
            {
                eb.HasNoKey();
                eb.ToView("vw_ActiveTransportationTrips");
            });

            builder.Entity<VwBookingSummary>(eb =>
            {
                eb.HasNoKey();
                eb.ToView("vw_TripBookingSummary");
            });
        }
    }
}

