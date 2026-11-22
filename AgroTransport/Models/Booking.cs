using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgroTransport.Models
{
    /// <summary>
    /// Represents a farmer's pooled transportation booking within a trip.
    /// Demonstrates Business Rule validation and monetary decimal handling (Unit II & V).
    /// </summary>
    public class Booking
    {
        [Key]
        [Display(Name = "Booking ID")]
        public int Id { get; set; }

        [Required]
        [StringLength(30)]
        [Display(Name = "Booking Reference")]
        public string BookingReference { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Trip ID")]
        public int TripId { get; set; }

        [ForeignKey(nameof(TripId))]
        public virtual TransportTrip? Trip { get; set; }

        [Required]
        [Display(Name = "Farmer")]
        public string FarmerId { get; set; } = string.Empty;

        [ForeignKey(nameof(FarmerId))]
        public virtual ApplicationUser? Farmer { get; set; }

        [Required(ErrorMessage = "Produce type is required.")]
        [StringLength(100)]
        [Display(Name = "Agricultural Produce")]
        public string ProduceType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, 50000, ErrorMessage = "Requested quantity must be at least 1 kg.")]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Requested Quantity (kg)")]
        public decimal RequestedQuantityKg { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Cost per kg (₹)")]
        public decimal CostPerKg { get; set; }

        /// <summary>
        /// Individual Cost = RequestedQuantityKg * CostPerKg
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(12,2)")]
        [Display(Name = "Total Calculated Cost (₹)")]
        public decimal TotalCost { get; set; }

        [StringLength(150)]
        [Display(Name = "Pickup Village / Landmark")]
        public string? PickupVillage { get; set; }

        [StringLength(150)]
        [Display(Name = "Dropoff Location")]
        public string? DropoffLocation { get; set; }

        [StringLength(500)]
        [Display(Name = "Special Instructions")]
        public string? SpecialInstructions { get; set; }

        [StringLength(300)]
        [Display(Name = "Transporter Remarks")]
        public string? TransporterRemarks { get; set; }

        /// <summary>
        /// Status: Pending, Accepted, Rejected, Cancelled, Completed
        /// </summary>
        [Required]
        [StringLength(30)]
        [Display(Name = "Booking Status")]
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}

