using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgroTransport.Data;
using AgroTransport.Models;
using AgroTransport.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgroTransport.Services
{
    public class TripService : ITripService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICostCalculationService _costCalculator;

        public TripService(ApplicationDbContext context, ICostCalculationService costCalculator)
        {
            _context = context;
            _costCalculator = costCalculator;
        }

        public async Task<List<TransportTrip>> GetAvailableTripsAsync(
            string? source = null,
            string? destination = null,
            string? mandi = null,
            DateTime? departureDate = null,
            string? vehicleType = null,
            decimal? minCapacity = null)
        {
            var query = _context.TransportTrips
                .Include(t => t.Vehicle)
                .Include(t => t.Transporter)
                .Include(t => t.Bookings.Where(b => b.Status == "Accepted"))
                .Where(t => t.TripStatus == "Scheduled" && t.AvailableCapacityKg > 0);

            // Filter: Departure date >= today
            var today = DateTime.UtcNow.Date;
            query = query.Where(t => t.DepartureDate.Date >= today);

            // Filter: Source location
            if (!string.IsNullOrWhiteSpace(source))
            {
                var s = source.Trim();
                query = query.Where(t => EF.Functions.Like(t.SourceLocation, $"%{s}%"));
            }

            // Filter: Destination location
            if (!string.IsNullOrWhiteSpace(destination))
            {
                var d = destination.Trim();
                query = query.Where(t => EF.Functions.Like(t.DestinationLocation, $"%{d}%"));
            }

            // Filter: Mandi Name
            if (!string.IsNullOrWhiteSpace(mandi))
            {
                var m = mandi.Trim();
                query = query.Where(t => EF.Functions.Like(t.MandiName, $"%{m}%"));
            }

            // Filter: Specific Departure Date
            if (departureDate.HasValue)
            {
                var dateOnly = departureDate.Value.Date;
                query = query.Where(t => t.DepartureDate.Date == dateOnly);
            }

            // Filter: Vehicle Type
            if (!string.IsNullOrWhiteSpace(vehicleType) && vehicleType != "All")
            {
                query = query.Where(t => t.Vehicle != null && t.Vehicle.VehicleType == vehicleType);
            }

            // Filter: Minimum Available Capacity
            if (minCapacity.HasValue && minCapacity.Value > 0)
            {
                query = query.Where(t => t.AvailableCapacityKg >= minCapacity.Value);
            }

            return await query
                .OrderBy(t => t.DepartureDate)
                .ThenBy(t => t.CostPerKg)
                .ToListAsync();
        }

        public async Task<TransportTrip?> GetTripDetailsAsync(int tripId)
        {
            return await _context.TransportTrips
                .Include(t => t.Vehicle)
                .Include(t => t.Transporter)
                .Include(t => t.Bookings)
                    .ThenInclude(b => b.Farmer)
                .FirstOrDefaultAsync(t => t.Id == tripId);
        }

        public async Task<List<TransportTrip>> GetTripsByTransporterAsync(string transporterId)
        {
            return await _context.TransportTrips
                .Include(t => t.Vehicle)
                .Include(t => t.Bookings)
                .Where(t => t.TransporterId == transporterId)
                .OrderByDescending(t => t.DepartureDate)
                .ToListAsync();
        }

        public async Task<(bool Success, string Message, int TripId)> CreateTripAsync(TransportTrip trip)
        {
            // Business Rule: Departure date must not be in the past
            if (trip.DepartureDate.Date < DateTime.UtcNow.Date)
            {
                return (false, "Trip departure date cannot be in the past.", 0);
            }

            var vehicle = await _context.Vehicles.FindAsync(trip.VehicleId);
            if (vehicle == null)
            {
                return (false, "Selected vehicle does not exist.", 0);
            }

            if (vehicle.TransporterId != trip.TransporterId)
            {
                return (false, "You can only schedule trips with vehicles registered under your account.", 0);
            }

            if (vehicle.Status != "Active")
            {
                return (false, "Selected vehicle is currently not marked as Active.", 0);
            }

            // Recalculate server-side: Total capacity and cost per kg
            trip.TotalCapacityKg = vehicle.CapacityKg;
            trip.AvailableCapacityKg = vehicle.CapacityKg;
            trip.CostPerKg = _costCalculator.CalculateCostPerKg(trip.TotalCost, trip.TotalCapacityKg);
            trip.TripStatus = "Scheduled";
            trip.CreatedAt = DateTime.UtcNow;
            trip.ConcurrencyStamp = Guid.NewGuid();

            _context.TransportTrips.Add(trip);
            await _context.SaveChangesAsync();

            return (true, "Transportation trip published successfully! It is now visible to farmers for pooling.", trip.Id);
        }

        public async Task<(bool Success, string Message)> UpdateTripStatusAsync(int tripId, string transporterId, string newStatus)
        {
            var trip = await _context.TransportTrips
                .Include(t => t.Bookings)
                .FirstOrDefaultAsync(t => t.Id == tripId);

            if (trip == null)
                return (false, "Trip not found.");

            if (trip.TransporterId != transporterId)
                return (false, "Unauthorized to modify this trip.");

            trip.TripStatus = newStatus;

            // If trip completed, mark accepted bookings as completed
            if (newStatus == "Completed")
            {
                foreach (var b in trip.Bookings.Where(b => b.Status == "Accepted"))
                {
                    b.Status = "Completed";
                    b.UpdatedAt = DateTime.UtcNow;
                }
            }
            // If trip cancelled, cancel active bookings
            else if (newStatus == "Cancelled")
            {
                foreach (var b in trip.Bookings.Where(b => b.Status == "Accepted" || b.Status == "Pending"))
                {
                    b.Status = "Cancelled";
                    b.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
            return (true, $"Trip status updated to {newStatus}.");
        }

        public async Task<List<TransportTrip>> GetAllTripsAdminAsync()
        {
            return await _context.TransportTrips
                .Include(t => t.Transporter)
                .Include(t => t.Vehicle)
                .Include(t => t.Bookings)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<(bool Success, string Message)> DeleteTripAsync(int tripId, bool isAdmin = false, string? transporterId = null)
        {
            var trip = await _context.TransportTrips
                .Include(t => t.Bookings)
                .FirstOrDefaultAsync(t => t.Id == tripId);

            if (trip == null)
                return (false, "Trip not found.");

            if (!isAdmin && trip.TransporterId != transporterId)
                return (false, "Unauthorized to delete this trip.");

            if (trip.Bookings.Any(b => b.Status == "Accepted"))
            {
                return (false, "Cannot delete trip because active accepted bookings exist. Please cancel the trip instead.");
            }

            _context.TransportTrips.Remove(trip);
            await _context.SaveChangesAsync();

            return (true, "Trip removed successfully.");
        }

        public async Task<List<VwActiveTrip>> GetActiveTripsViewAsync()
        {
            return await _context.VwActiveTrips.ToListAsync();
        }

        public async Task<List<VwBookingSummary>> GetBookingSummaryViewAsync()
        {
            return await _context.VwBookingSummaries.ToListAsync();
        }
    }
}

