using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgroTransport.Models
{
    /// <summary>
    /// Represents database-stored real notifications for platform events (Unit V).
    /// </summary>
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser? User { get; set; }

        [Required]
        [StringLength(120)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Message { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;

        [StringLength(50)]
        public string NotificationType { get; set; } = "General"; // BookingSubmitted, BookingAccepted, BookingRejected, BookingCancelled, TripScheduled, TripCompleted

        public int? RelatedEntityId { get; set; }

        [StringLength(250)]
        public string? LinkUrl { get; set; }
    }
}

