using System.Collections.Generic;
using AgroTransport.Models;

namespace AgroTransport.ViewModels.Admin
{
    public class AdminDashboardViewModel
    {
        public int TotalFarmers { get; set; }
        public int TotalTransporters { get; set; }
        public int TotalVehicles { get; set; }
        public int TotalTrips { get; set; }
        public int ActiveTrips { get; set; }
        public int TotalBookings { get; set; }
        public int PendingBookings { get; set; }
        public int CompletedBookings { get; set; }
        public decimal TotalPlatformVolumeKg { get; set; }
        public decimal TotalEconomicValue { get; set; }

        public List<ApplicationUser> RecentUsers { get; set; } = new List<ApplicationUser>();
        public List<TransportTrip> RecentTrips { get; set; } = new List<TransportTrip>();
        public List<Booking> RecentBookings { get; set; } = new List<Booking>();
        public List<Vehicle> AllVehicles { get; set; } = new List<Vehicle>();

        // Chart Data Properties for Chart.js
        public List<string> ChartLabels { get; set; } = new List<string>();
        public List<decimal> ChartBookedCapacityData { get; set; } = new List<decimal>();
        public List<decimal> ChartRevenueData { get; set; } = new List<decimal>();
    }
}

