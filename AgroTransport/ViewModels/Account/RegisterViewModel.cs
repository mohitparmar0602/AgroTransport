using System.ComponentModel.DataAnnotations;

namespace AgroTransport.ViewModels.Account
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 100 characters.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mobile Number is required.")]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Please enter a valid 10-digit Indian mobile number.")]
        [Display(Name = "Mobile Number")]
        public string MobileNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a role.")]
        [RegularExpression("^(Farmer|Transporter)$", ErrorMessage = "Role must be either Farmer or Transporter.")]
        [Display(Name = "I am a")]
        public string UserRole { get; set; } = "Farmer";

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Village / Town is required.")]
        [StringLength(100)]
        [Display(Name = "Village / Town")]
        public string Village { get; set; } = string.Empty;

        [Required(ErrorMessage = "District is required.")]
        [StringLength(100)]
        [Display(Name = "District")]
        public string District { get; set; } = string.Empty;

        [Required(ErrorMessage = "State is required.")]
        [StringLength(100)]
        [Display(Name = "State")]
        public string State { get; set; } = "Punjab";

        [StringLength(250)]
        [Display(Name = "Detailed Address (Optional)")]
        public string? Address { get; set; }
    }
}

