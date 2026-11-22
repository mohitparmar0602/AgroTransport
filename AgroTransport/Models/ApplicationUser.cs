using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace AgroTransport.Models
{
    /// <summary>
    /// Custom Identity User extending IdentityUser for AgroTransport platform.
    /// Demonstrates C# Object-Oriented Inheritance (Unit II) and ASP.NET Core Identity (Unit IV).
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Roles: Farmer, Transporter, Administrator
        /// </summary>
        [Required]
        [StringLength(30)]
        [Display(Name = "User Role")]
        public string UserRole { get; set; } = "Farmer";

        [StringLength(100)]
        public string? Village { get; set; }

        [StringLength(100)]
        public string? District { get; set; }

        [StringLength(100)]
        public string? State { get; set; }

        [StringLength(250)]
        public string? Address { get; set; }

        public DateTime ProfileCreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
        public virtual ICollection<TransportTrip> Trips { get; set; } = new List<TransportTrip>();
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public virtual ICollection<TransportRequest> TransportRequests { get; set; } = new List<TransportRequest>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}

