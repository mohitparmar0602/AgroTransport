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
    /// <summary>
    /// Service implementing the Core Transportation Pooling Logic (Feature 6 & 7).
    /// Enforces transaction safety, optimistic concurrency, and dynamic capacity recalculation.
    /// Demonstrates Unit II, IV and V syllabus requirements.
    /// </summary>
    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICostCalculationService _costCalculator;
        private readonly INotificationService _notificationService;

        public BookingService(
            ApplicationDbContext context,
            ICostCalculationService costCalculator,
            INotificationService notificationService)
        {
            _context = context;
            _costCalculator = costCalculator;
            _notificationService = notificationService;
        }

        public async Task<(bool Success, string Message, int BookingId)> CreateBookingRequestAsync(
            int tripId,
            string farmerId,
            string produceType,
            decimal requestedQuantityKg,
            string? pickupVillage,
            string? dropoffLocation,
            string? specialInstructions)
        {
            // Rule 7: Booking quantity must be greater than zero
            if (requestedQuantityKg <= 0)
            {
                return (false, "Requested quantity must be greater than zero kilograms.", 0);
            }

            var trip = await _context.TransportTrips
                .Include(t => t.Transporter)
                .FirstOrDefaultAsync(t => t.Id == tripId);

            if (trip == null)
            {
                return (false, "Transportation trip was not found.", 0);
            }

            // Rule 14: Inactive trips must not be bookable
            if (trip.TripStatus != "Scheduled")
            {
                return (false, $"This trip is currently '{trip.TripStatus}' and cannot accept new bookings.", 0);
            }

            // Rule 6: Trip departure dates must not be in the past
            if (trip.DepartureDate.Date < DateTime.UtcNow.Date)
            {
                return (false, "Cannot book trips whose departure date has already passed.", 0);
            }

            // Rule 15: Transporters cannot book their own trips as farmers
            if (trip.TransporterId == farmerId)
            {
                return (false, "You cannot book your own transportation trip.", 0);
            }

            // Rule 8: Requested quantity must not exceed available capacity
            if (requestedQuantityKg > trip.AvailableCapacityKg)
            {
                return (false, $"Requested quantity ({requestedQuantityKg:N0} kg) exceeds remaining available vehicle capacity ({trip.AvailableCapacityKg:N0} kg).", 0);
            }

            // Rule: Prevent duplicate active bookings by the same farmer for the same trip
            bool hasDuplicate = await _context.Bookings
                .AnyAsync(b => b.TripId == tripId && b.FarmerId == farmerId && (b.Status == "Pending" || b.Status == "Accepted"));

            if (hasDuplicate)
            {
                return (false, "You already have an active (pending or accepted) booking for this trip.", 0);
            }

            // Recalculate cost strictly on server side (Never trust browser values)
            decimal costPerKg = trip.CostPerKg > 0 
                ? trip.CostPerKg 
                : _costCalculator.CalculateCostPerKg(trip.TotalCost, trip.TotalCapacityKg);
            decimal totalCalculatedCost = _costCalculator.CalculateFarmerShare(requestedQuantityKg, costPerKg);

            // Generate Unique Booking Reference
            string reference = $"BK-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}";

            var booking = new Booking
            {
                BookingReference = reference,
                TripId = tripId,
                FarmerId = farmerId,
                ProduceType = produceType.Trim(),
                RequestedQuantityKg = requestedQuantityKg,
                CostPerKg = costPerKg,
                TotalCost = totalCalculatedCost,
                PickupVillage = pickupVillage?.Trim(),
                DropoffLocation = dropoffLocation?.Trim(),
                SpecialInstructions = specialInstructions?.Trim(),
                Status = "Pending", // Rule 10: Pending booking does NOT immediately reduce confirmed capacity until accepted
                CreatedAt = DateTime.UtcNow
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            // Notify Transporter of new booking request
            var farmer = await _context.Users.FindAsync(farmerId);
            string farmerName = farmer?.FullName ?? "A farmer";
            await _notificationService.CreateNotificationAsync(
                trip.TransporterId,
                "New Booking Request",
                $"{farmerName} requested to pool {requestedQuantityKg:N0} kg of {produceType} on your trip from {trip.SourceLocation} to {trip.DestinationLocation}.",
                "BookingSubmitted",
                booking.Id,
                $"/Transporter/TripBookings/{trip.Id}");

            return (true, "Booking request submitted successfully! The transporter will review and accept your request.", booking.Id);
        }

        public async Task<(bool Success, string Message)> AcceptBookingAsync(int bookingId, string transporterId, string? remarks)
        {
            // Use database transaction for concurrency safety and capacity integrity
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var booking = await _context.Bookings
                    .Include(b => b.Trip)
                    .Include(b => b.Farmer)
                    .FirstOrDefaultAsync(b => b.Id == bookingId);

                if (booking == null)
                    return (false, "Booking not found.");

                if (booking.Trip == null)
                    return (false, "Associated trip not found.");

                // Authorization check
                if (booking.Trip.TransporterId != transporterId)
                    return (false, "Unauthorized: You do not own this transportation trip.");

                if (booking.Status != "Pending")
                    return (false, $"Cannot accept booking with current status '{booking.Status}'.");

                // Rule 8 & 11: Validate available capacity before accepting
                if (booking.RequestedQuantityKg > booking.Trip.AvailableCapacityKg)
                {
                    return (false, $"Insufficient remaining capacity! Trip available capacity is {booking.Trip.AvailableCapacityKg:N0} kg, but requested is {booking.RequestedQuantityKg:N0} kg.");
                }

                // Update Trip Available Capacity (Rule 11)
                booking.Trip.AvailableCapacityKg -= booking.RequestedQuantityKg;
                booking.Trip.ConcurrencyStamp = Guid.NewGuid();

                // Update Booking Status
                booking.Status = "Accepted";
                booking.TransporterRemarks = remarks;
                booking.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Notify Farmer that booking was accepted
                await _notificationService.CreateNotificationAsync(
                    booking.FarmerId,
                    "Booking Accepted!",
                    $"Your pooled transport booking ({booking.BookingReference}) for {booking.RequestedQuantityKg:N0} kg of {booking.ProduceType} has been accepted by {booking.Trip.Transporter?.FullName ?? "transporter"}.",
                    "BookingAccepted",
                    booking.Id,
                    $"/Bookings/Details/{booking.Id}");

                return (true, $"Booking {booking.BookingReference} accepted. Remaining vehicle capacity is now {booking.Trip.AvailableCapacityKg:N0} kg.");
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                return (false, "Conflict detected: The trip capacity was modified by another operation simultaneously. Please refresh and try again.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Failed to accept booking: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> RejectBookingAsync(int bookingId, string transporterId, string? reason)
        {
            var booking = await _context.Bookings
                .Include(b => b.Trip)
                .Include(b => b.Farmer)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                return (false, "Booking not found.");

            if (booking.Trip == null)
                return (false, "Associated trip not found.");

            if (booking.Trip.TransporterId != transporterId)
                return (false, "Unauthorized: You do not own this transportation trip.");

            if (booking.Status != "Pending")
                return (false, $"Cannot reject booking with current status '{booking.Status}'.");

            // Rule 9: A rejected booking must NOT reduce or change available capacity
            booking.Status = "Rejected";
            booking.TransporterRemarks = reason;
            booking.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Notify Farmer of rejection
            await _notificationService.CreateNotificationAsync(
                booking.FarmerId,
                "Booking Request Rejected",
                $"Your booking request ({booking.BookingReference}) was declined by the transporter. Reason: {reason ?? "Vehicle capacity allocation full or route adjustment."}",
                "BookingRejected",
                booking.Id,
                $"/Bookings/Details/{booking.Id}");

            return (true, "Booking request has been rejected.");
        }

        public async Task<(bool Success, string Message)> CancelBookingAsync(int bookingId, string userId, bool isAdmin = false)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var booking = await _context.Bookings
                    .Include(b => b.Trip)
                    .FirstOrDefaultAsync(b => b.Id == bookingId);

                if (booking == null)
                    return (false, "Booking not found.");

                // Authorization check: Only booking farmer, trip transporter, or admin can cancel
                bool isFarmer = booking.FarmerId == userId;
                bool isTransporter = booking.Trip?.TransporterId == userId;

                if (!isAdmin && !isFarmer && !isTransporter)
                {
                    return (false, "Unauthorized to cancel this booking.");
                }

                // Rule 13: Completed bookings must not be cancelled
                if (booking.Status == "Completed")
                {
                    return (false, "Completed bookings cannot be cancelled.");
                }

                if (booking.Status == "Cancelled" || booking.Status == "Rejected")
                {
                    return (false, $"Booking is already {booking.Status.ToLower()}.");
                }

                // Rule 12: If booking was Accepted, restore vehicle available capacity
                if (booking.Status == "Accepted" && booking.Trip != null)
                {
                    booking.Trip.AvailableCapacityKg = Math.Min(
                        booking.Trip.TotalCapacityKg, 
                        booking.Trip.AvailableCapacityKg + booking.RequestedQuantityKg);
                    booking.Trip.ConcurrencyStamp = Guid.NewGuid();
                }

                string previousStatus = booking.Status;
                booking.Status = "Cancelled";
                booking.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Send appropriate notification
                if (isFarmer && booking.Trip != null)
                {
                    await _notificationService.CreateNotificationAsync(
                        booking.Trip.TransporterId,
                        "Booking Cancelled by Farmer",
                        $"Farmer cancelled booking {booking.BookingReference} for {booking.RequestedQuantityKg:N0} kg. Vehicle capacity has been restored.",
                        "BookingCancelled",
                        booking.Id,
                        $"/Transporter/TripBookings/{booking.TripId}");
                }
                else if (isTransporter)
                {
                    await _notificationService.CreateNotificationAsync(
                        booking.FarmerId,
                        "Booking Cancelled by Transporter",
                        $"Transporter cancelled booking {booking.BookingReference}.",
                        "BookingCancelled",
                        booking.Id,
                        $"/Bookings/Details/{booking.Id}");
                }

                return (true, $"Booking cancelled successfully. {(previousStatus == "Accepted" ? "Vehicle capacity was safely restored." : "")}");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Failed to cancel booking: {ex.Message}");
            }
        }

        public async Task<List<Booking>> GetFarmerBookingsAsync(string farmerId)
        {
            return await _context.Bookings
                .Include(b => b.Trip)
                    .ThenInclude(t => t!.Vehicle)
                .Include(b => b.Trip)
                    .ThenInclude(t => t!.Transporter)
                .Where(b => b.FarmerId == farmerId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Booking>> GetTransporterBookingsAsync(string transporterId)
        {
            return await _context.Bookings
                .Include(b => b.Farmer)
                .Include(b => b.Trip)
                    .ThenInclude(t => t!.Vehicle)
                .Where(b => b.Trip != null && b.Trip.TransporterId == transporterId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Booking>> GetTripBookingsAsync(int tripId, string transporterId)
        {
            return await _context.Bookings
                .Include(b => b.Farmer)
                .Include(b => b.Trip)
                .Where(b => b.TripId == tripId && b.Trip != null && b.Trip.TransporterId == transporterId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<Booking?> GetBookingByIdAsync(int bookingId)
        {
            return await _context.Bookings
                .Include(b => b.Farmer)
                .Include(b => b.Trip)
                    .ThenInclude(t => t!.Vehicle)
                .Include(b => b.Trip)
                    .ThenInclude(t => t!.Transporter)
                .FirstOrDefaultAsync(b => b.Id == bookingId);
        }

        public async Task<List<Booking>> GetAllBookingsAdminAsync()
        {
            return await _context.Bookings
                .Include(b => b.Farmer)
                .Include(b => b.Trip)
                    .ThenInclude(t => t!.Transporter)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }
    }
}

