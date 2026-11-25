using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace AgroTransport.Models
{
    /// <summary>
    /// Represents a transportation requirement broadcasted by a farmer when no suitable trip is scheduled yet.
    /// Transporters can review requests and organize new trips to fulfill them.
    /// </summary>
    public class TransportRequest
    {
        [Key]
        [Display(Name = "Request ID")]
        public int Id { get; set; }

        [ValidateNever]
        [Display(Name = "Farmer")]
        public string FarmerId { get; set; } = string.Empty;

        [ValidateNever]
        [ForeignKey(nameof(FarmerId))]
        public virtual ApplicationUser? Farmer { get; set; }

        [Required(ErrorMessage = "Pickup location is required.")]
        [StringLength(100)]
        [Display(Name = "Pickup Location / Village")]
        public string PickupLocation { get; set; } = string.Empty;

        [Required(ErrorMessage = "Destination location is required.")]
        [StringLength(100)]
        [Display(Name = "Destination / City")]
        public string DestinationLocation { get; set; } = string.Empty;

        [Required(ErrorMessage = "Target Mandi is required.")]
        [StringLength(120)]
        [Display(Name = "Target Mandi")]
        public string MandiName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Produce type is required.")]
        [StringLength(100)]
        [Display(Name = "Produce Type")]
        public string ProduceType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Required capacity is required.")]
        [Range(1, 50000, ErrorMessage = "Required capacity must be between 1 and 50,000 kg.")]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Required Capacity (kg)")]
        public decimal RequiredCapacityKg { get; set; }

        [Required(ErrorMessage = "Preferred date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Preferred Date")]
        public DateTime PreferredDate { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        [Display(Name = "Target Budget (₹)")]
        public decimal? EstimatedBudget { get; set; }

        [StringLength(500)]
        [Display(Name = "Additional Notes")]
        public string? AdditionalNotes { get; set; }

        /// <summary>
        /// Status: Open, Accepted, Fulfilled, Cancelled
        /// </summary>
        [ValidateNever]
        [StringLength(30)]
        [Display(Name = "Request Status")]
        public string Status { get; set; } = "Open";

        [ValidateNever]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

