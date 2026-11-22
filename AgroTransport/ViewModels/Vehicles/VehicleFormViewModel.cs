using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AgroTransport.ViewModels.Vehicles
{
    public class VehicleFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Registration number is required (e.g. PB-10-AB-1234).")]
        [RegularExpression(@"^[A-Z]{2}[-\s]?[0-9]{1,2}[-\s]?[A-Z]{1,3}[-\s]?[0-9]{4}$",
            ErrorMessage = "Please enter a valid vehicle registration number (e.g., PB-10-AB-1234).")]
        [Display(Name = "Vehicle Registration Number")]
        public string RegistrationNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vehicle type is required.")]
        [Display(Name = "Vehicle Type")]
        public string VehicleType { get; set; } = "Mini Truck";

        [Required(ErrorMessage = "Vehicle load capacity is required.")]
        [Range(100, 50000, ErrorMessage = "Load capacity must be between 100 kg and 50,000 kg.")]
        [Display(Name = "Max Load Capacity (kg)")]
        public decimal CapacityKg { get; set; }

        [Required(ErrorMessage = "Vehicle make & model is required.")]
        [StringLength(100)]
        [Display(Name = "Make & Model (e.g. Tata 407, Mahindra Bolero)")]
        public string Model { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Operational Status")]
        public string Status { get; set; } = "Active";

        public List<SelectListItem> VehicleTypes { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "Mini Truck", Text = "Mini Truck (1,000 - 2,500 kg)" },
            new SelectListItem { Value = "Pickup Truck", Text = "Pickup Truck (1,500 - 3,000 kg)" },
            new SelectListItem { Value = "Tractor Trolley", Text = "Tractor Trolley (3,000 - 6,000 kg)" },
            new SelectListItem { Value = "Medium Truck", Text = "Medium Truck (5,000 - 10,000 kg)" },
            new SelectListItem { Value = "Heavy Truck", Text = "Heavy Truck (10,000+ kg)" }
        };

        public List<SelectListItem> StatusOptions { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "Active", Text = "Active (Available for Trips)" },
            new SelectListItem { Value = "Maintenance", Text = "Under Maintenance" },
            new SelectListItem { Value = "Inactive", Text = "Inactive" }
        };
    }
}

