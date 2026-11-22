using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgroTransport.Models
{
    /// <summary>
    /// Represents an agricultural cargo vehicle owned by a Transporter.
    /// Demonstrates Entity Framework Core entity modeling and constraints (Unit V).
    /// </summary>
    public class Vehicle
    {
        [Key]
        [Display(Name = "Vehicle ID")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Registration number is required.")]
        [StringLength(20, ErrorMessage = "Registration number cannot exceed 20 characters.")]
        [Display(Name = "Registration Number")]
        public string RegistrationNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vehicle type is required.")]
        [StringLength(50)]
        [Display(Name = "Vehicle Type")]
        public string VehicleType { get; set; } = "Mini Truck"; // Mini Truck, Pickup Truck, Tractor Trolley, Medium Truck, Heavy Truck

        [Required(ErrorMessage = "Load capacity is required.")]
        [Range(100, 50000, ErrorMessage = "Capacity must be between 100 kg and 50,000 kg.")]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Load Capacity (kg)")]
        public decimal CapacityKg { get; set; }

        [Required(ErrorMessage = "Vehicle model is required.")]
        [StringLength(100)]
        [Display(Name = "Vehicle Model")]
        public string Model { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Owner")]
        public string TransporterId { get; set; } = string.Empty;

        [ForeignKey(nameof(TransporterId))]
        public virtual ApplicationUser? Transporter { get; set; }

        [Required]
        [StringLength(30)]
        [Display(Name = "Vehicle Status")]
        public string Status { get; set; } = "Active"; // Active, Maintenance, Inactive

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public virtual ICollection<TransportTrip> Trips { get; set; } = new List<TransportTrip>();
    }
}

