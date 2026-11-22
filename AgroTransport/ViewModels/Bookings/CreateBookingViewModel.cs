using System;
using System.ComponentModel.DataAnnotations;

namespace AgroTransport.ViewModels.Bookings
{
    public class CreateBookingViewModel
    {
        [Required]
        public int TripId { get; set; }

        public string SourceLocation { get; set; } = string.Empty;
        public string DestinationLocation { get; set; } = string.Empty;
        public string MandiName { get; set; } = string.Empty;
        public DateTime DepartureDate { get; set; }
        public TimeSpan DepartureTime { get; set; }
        public string VehicleType { get; set; } = string.Empty;
        public string VehicleModel { get; set; } = string.Empty;
        public string TransporterName { get; set; } = string.Empty;
        public decimal TotalCapacityKg { get; set; }
        public decimal AvailableCapacityKg { get; set; }
        public decimal TotalTripCost { get; set; }
        public decimal CostPerKg { get; set; }

        [Required(ErrorMessage = "Please specify the agricultural produce being transported.")]
        [StringLength(100, ErrorMessage = "Produce type cannot exceed 100 characters.")]
        [Display(Name = "Agricultural Produce Type (e.g. Potatoes, Wheat, Tomatoes)")]
        public string ProduceType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter the required load weight.")]
        [Range(1, 50000, ErrorMessage = "Requested weight must be at least 1 kg.")]
        [Display(Name = "Requested Load Weight (in Kilograms)")]
        public decimal RequestedQuantityKg { get; set; }

        [StringLength(150)]
        [Display(Name = "Farmer Pickup Village / Farm Landmark")]
        public string? PickupVillage { get; set; }

        [StringLength(150)]
        [Display(Name = "Mandi Dropoff Shed / Gate Location")]
        public string? DropoffLocation { get; set; }

        [StringLength(500)]
        [Display(Name = "Packaging Details & Special Instructions")]
        public string? SpecialInstructions { get; set; }
    }
}

