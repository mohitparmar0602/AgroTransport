using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgroTransport.Models
{
    /// <summary>
    /// Represents a scheduled agricultural transport trip published by a Transporter.
    /// Supports transportation pooling where multiple farmers share load capacity and cost.
    /// </summary>
    public class TransportTrip
    {
        [Key]
        [Display(Name = "Trip ID")]
        public int Id { get; set; }

        [Required]
        public string TransporterId { get; set; } = string.Empty;

        [ForeignKey(nameof(TransporterId))]
        public virtual ApplicationUser? Transporter { get; set; }

        [Required]
        [Display(Name = "Vehicle")]
        public int VehicleId { get; set; }

        [ForeignKey(nameof(VehicleId))]
        public virtual Vehicle? Vehicle { get; set; }

        [Required(ErrorMessage = "Source pickup location is required.")]
        [StringLength(100)]
        [Display(Name = "Source Location")]
        public string SourceLocation { get; set; } = string.Empty;

        [Required(ErrorMessage = "Destination location is required.")]
        [StringLength(100)]
        [Display(Name = "Destination Location")]
        public string DestinationLocation { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mandi name is required.")]
        [StringLength(120)]
        [Display(Name = "Agricultural Mandi")]
        public string MandiName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Departure date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Departure Date")]
        public DateTime DepartureDate { get; set; }

        [Required(ErrorMessage = "Departure time is required.")]
        [Display(Name = "Departure Time")]
        public TimeSpan DepartureTime { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Total Capacity (kg)")]
        public decimal TotalCapacityKg { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Available Capacity (kg)")]
        public decimal AvailableCapacityKg { get; set; }

        [Required]
        [Column(TypeName = "decimal(12,2)")]
        [Range(1, 1000000, ErrorMessage = "Total cost must be greater than zero.")]
        [Display(Name = "Total Trip Cost (₹)")]
        public decimal TotalCost { get; set; }

        /// <summary>
        /// Cost per kg = TotalCost / TotalCapacityKg
        /// Formula required by Feature 6: Transportation Pooling
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Cost per kg (₹)")]
        public decimal CostPerKg { get; set; }

        [StringLength(500)]
        [Display(Name = "Description / Route Notes")]
        public string? Description { get; set; }

        [Required]
        [StringLength(30)]
        [Display(Name = "Trip Status")]
        public string TripStatus { get; set; } = "Scheduled"; // Scheduled, InTransit, Completed, Cancelled

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Concurrency token to protect against race conditions and simultaneous overbooking
        /// </summary>
        [ConcurrencyCheck]
        public Guid ConcurrencyStamp { get; set; } = Guid.NewGuid();

        // Navigation
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}

