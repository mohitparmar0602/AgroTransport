using System.Collections.Generic;
using AgroTransport.Models;

namespace AgroTransport.ViewModels.Transporter
{
    public class TransporterDashboardViewModel
    {
        public string TransporterName { get; set; } = string.Empty;
        public int TotalVehicles { get; set; }
        public int TotalTrips { get; set; }
        public int ActiveTrips { get; set; }
        public int CompletedTrips { get; set; }
        public int PendingRequestsCount { get; set; }
        public decimal TotalEarnings { get; set; }
        public decimal TotalCapacityManagedKg { get; set; }

        public List<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
        public List<TransportTrip> Trips { get; set; } = new List<TransportTrip>();
        public List<Booking> PendingBookings { get; set; } = new List<Booking>();
        public List<Notification> RecentNotifications { get; set; } = new List<Notification>();
        public List<TransportRequest> OpenMarketRequests { get; set; } = new List<TransportRequest>();
    }
}

