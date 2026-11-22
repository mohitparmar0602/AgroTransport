using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgroTransport.Models
{
    /// <summary>
    /// Model mapped to SQL View: vw_ActiveTransportationTrips
    /// Demonstrates Unit V SQL Views integration in EF Core.
    /// </summary>
    public class VwActiveTrip
    {
        public int TripId { get; set; }
        public string TransporterName { get; set; } = string.Empty;
        public string TransporterEmail { get; set; } = string.Empty;
        public string VehicleRegistration { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public string SourceLocation { get; set; } = string.Empty;
        public string DestinationLocation { get; set; } = string.Empty;
        public string MandiName { get; set; } = string.Empty;
        public DateTime DepartureDate { get; set; }
        public TimeSpan DepartureTime { get; set; }
        public decimal TotalCapacityKg { get; set; }
        public decimal AvailableCapacityKg { get; set; }
        public decimal TotalCost { get; set; }
        public decimal CostPerKg { get; set; }
        public string TripStatus { get; set; } = string.Empty;
        public int ActiveBookingsCount { get; set; }
    }
}

