using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgroTransport.Models;

namespace AgroTransport.Services.Interfaces
{
    public interface ITripService
    {
        Task<List<TransportTrip>> GetAvailableTripsAsync(
            string? source = null,
            string? destination = null,
            string? mandi = null,
            DateTime? departureDate = null,
            string? vehicleType = null,
            decimal? minCapacity = null);

        Task<TransportTrip?> GetTripDetailsAsync(int tripId);
        Task<List<TransportTrip>> GetTripsByTransporterAsync(string transporterId);
        Task<(bool Success, string Message, int TripId)> CreateTripAsync(TransportTrip trip);
        Task<(bool Success, string Message)> UpdateTripStatusAsync(int tripId, string transporterId, string newStatus);
        Task<List<TransportTrip>> GetAllTripsAdminAsync();
        Task<(bool Success, string Message)> DeleteTripAsync(int tripId, bool isAdmin = false, string? transporterId = null);
        Task<List<VwActiveTrip>> GetActiveTripsViewAsync();
        Task<List<VwBookingSummary>> GetBookingSummaryViewAsync();
    }
}

