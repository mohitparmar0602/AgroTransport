using System.Collections.Generic;
using System.Threading.Tasks;
using AgroTransport.Models;

namespace AgroTransport.Services.Interfaces
{
    public interface IBookingService
    {
        Task<(bool Success, string Message, int BookingId)> CreateBookingRequestAsync(
            int tripId,
            string farmerId,
            string produceType,
            decimal requestedQuantityKg,
            string? pickupVillage,
            string? dropoffLocation,
            string? specialInstructions);

        Task<(bool Success, string Message)> AcceptBookingAsync(int bookingId, string transporterId, string? remarks);
        Task<(bool Success, string Message)> RejectBookingAsync(int bookingId, string transporterId, string? reason);
        Task<(bool Success, string Message)> CancelBookingAsync(int bookingId, string userId, bool isAdmin = false);
        Task<List<Booking>> GetFarmerBookingsAsync(string farmerId);
        Task<List<Booking>> GetTransporterBookingsAsync(string transporterId);
        Task<List<Booking>> GetTripBookingsAsync(int tripId, string transporterId);
        Task<Booking?> GetBookingByIdAsync(int bookingId);
        Task<List<Booking>> GetAllBookingsAdminAsync();
    }
}

