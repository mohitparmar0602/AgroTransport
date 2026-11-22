using System;
using System.ComponentModel.DataAnnotations;

namespace AgroTransport.DTOs
{
    public class VehicleDto
    {
        public int Id { get; set; }

        [Required]
        public string RegistrationNumber { get; set; } = string.Empty;

        [Required]
        public string VehicleType { get; set; } = string.Empty;

        [Range(100, 50000)]
        public decimal CapacityKg { get; set; }

        [Required]
        public string Model { get; set; } = string.Empty;

        public string Status { get; set; } = "Active";

        public string? TransporterId { get; set; }
        public string? TransporterName { get; set; }
    }

    public class TripDto
    {
        public int Id { get; set; }
        public string? TransporterId { get; set; }
        public string? TransporterName { get; set; }
        public int VehicleId { get; set; }
        public string? VehicleRegistration { get; set; }
        public string SourceLocation { get; set; } = string.Empty;
        public string DestinationLocation { get; set; } = string.Empty;
        public string MandiName { get; set; } = string.Empty;
        public DateTime DepartureDate { get; set; }
        public TimeSpan DepartureTime { get; set; }
        public decimal TotalCapacityKg { get; set; }
        public decimal AvailableCapacityKg { get; set; }
        public decimal TotalCost { get; set; }
        public decimal CostPerKg { get; set; }
        public string? Description { get; set; }
        public string TripStatus { get; set; } = "Scheduled";
    }

    public class CreateTripDto
    {
        [Required]
        public int VehicleId { get; set; }

        [Required]
        public string SourceLocation { get; set; } = string.Empty;

        [Required]
        public string DestinationLocation { get; set; } = string.Empty;

        [Required]
        public string MandiName { get; set; } = string.Empty;

        [Required]
        public DateTime DepartureDate { get; set; }

        [Required]
        public TimeSpan DepartureTime { get; set; }

        [Range(100, 1000000)]
        public decimal TotalCost { get; set; }

        public string? Description { get; set; }
    }

    public class BookingDto
    {
        public int Id { get; set; }
        public string BookingReference { get; set; } = string.Empty;
        public int TripId { get; set; }
        public string? Route { get; set; }
        public string? FarmerId { get; set; }
        public string? FarmerName { get; set; }
        public string ProduceType { get; set; } = string.Empty;
        public decimal RequestedQuantityKg { get; set; }
        public decimal CostPerKg { get; set; }
        public decimal TotalCost { get; set; }
        public string? PickupVillage { get; set; }
        public string? DropoffLocation { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; }
    }

    public class CreateBookingDto
    {
        [Required]
        public int TripId { get; set; }

        [Required]
        public string ProduceType { get; set; } = string.Empty;

        [Range(1, 50000)]
        public decimal RequestedQuantityKg { get; set; }

        public string? PickupVillage { get; set; }
        public string? DropoffLocation { get; set; }
        public string? SpecialInstructions { get; set; }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public static ApiResponse<T> Ok(T data, string message = "Success") =>
            new ApiResponse<T> { Success = true, Message = message, Data = data };

        public static ApiResponse<T> Fail(string message) =>
            new ApiResponse<T> { Success = false, Message = message, Data = default };
    }
}

