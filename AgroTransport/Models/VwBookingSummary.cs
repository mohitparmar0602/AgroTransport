using System;

namespace AgroTransport.Models
{
    /// <summary>
    /// Model mapped to SQL View: vw_TripBookingSummary
    /// Demonstrates Unit V SQL Aggregations and Joins.
    /// </summary>
    public class VwBookingSummary
    {
        public int TripId { get; set; }
        public string Route { get; set; } = string.Empty;
        public string TransporterName { get; set; } = string.Empty;
        public decimal TotalCapacityKg { get; set; }
        public decimal TotalBookedKg { get; set; }
        public decimal AvailableCapacityKg { get; set; }
        public int TotalBookingsCount { get; set; }
        public int AcceptedBookingsCount { get; set; }
        public decimal TotalCollectedRevenue { get; set; }
        public decimal TripTotalCost { get; set; }
    }
}

