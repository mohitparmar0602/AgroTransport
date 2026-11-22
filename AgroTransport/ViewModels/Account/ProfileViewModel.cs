using System.ComponentModel.DataAnnotations;

namespace AgroTransport.ViewModels.Account
{
    public class ProfileViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Mobile Number")]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Please enter a valid 10-digit mobile number.")]
        public string PhoneNumber { get; set; } = string.Empty;

        public string UserRole { get; set; } = string.Empty;

        [Display(Name = "Village / Town")]
        public string? Village { get; set; }

        [Display(Name = "District")]
        public string? District { get; set; }

        [Display(Name = "State")]
        public string? State { get; set; }

        [Display(Name = "Full Address")]
        public string? Address { get; set; }
    }
}

