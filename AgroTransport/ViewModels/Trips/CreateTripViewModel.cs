using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AgroTransport.ViewModels.Trips
{
    public class CreateTripViewModel
    {
        [Required(ErrorMessage = "Please select a vehicle.")]
        [Display(Name = "Select Vehicle")]
        public int VehicleId { get; set; }

        [Required(ErrorMessage = "Source pickup location is required.")]
        [StringLength(100)]
        [Display(Name = "Source Location / Village / City")]
        public string SourceLocation { get; set; } = string.Empty;

        [Required(ErrorMessage = "Destination city is required.")]
        [StringLength(100)]
        [Display(Name = "Destination City / District")]
        public string DestinationLocation { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mandi / Market name is required.")]
        [StringLength(120)]
        [Display(Name = "Agricultural Mandi Name")]
        public string MandiName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Departure date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Departure Date")]
        public DateTime DepartureDate { get; set; } = DateTime.UtcNow.Date.AddDays(1);

        [Required(ErrorMessage = "Departure time is required.")]
        [Display(Name = "Departure Time (HH:mm)")]
        public TimeSpan DepartureTime { get; set; } = new TimeSpan(6, 0, 0);

        [Required(ErrorMessage = "Total trip cost is required.")]
        [Range(100, 1000000, ErrorMessage = "Total cost must be between ₹100 and ₹10,00,000.")]
        [Display(Name = "Total Trip Transportation Cost (₹)")]
        public decimal TotalCost { get; set; }

        [StringLength(500)]
        [Display(Name = "Route Notes & Instructions")]
        public string? Description { get; set; }

        public List<SelectListItem> AvailableVehicles { get; set; } = new List<SelectListItem>();
    }
}

