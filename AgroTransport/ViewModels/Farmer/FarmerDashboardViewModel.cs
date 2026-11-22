using System.Collections.Generic;
using AgroTransport.Models;

namespace AgroTransport.ViewModels.Farmer
{
    public class FarmerDashboardViewModel
    {
        public string FarmerName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int TotalBookings { get; set; }
        public int ActiveBookings { get; set; }
        public int PendingBookings { get; set; }
        public int CompletedBookings { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal TotalEstimatedSavings { get; set; }

        public List<Booking> RecentBookings { get; set; } = new List<Booking>();
        public List<TransportTrip> AvailableTrips { get; set; } = new List<TransportTrip>();
        public List<Notification> RecentNotifications { get; set; } = new List<Notification>();
        public List<TransportRequest> MyRequests { get; set; } = new List<TransportRequest>();
    }
}

