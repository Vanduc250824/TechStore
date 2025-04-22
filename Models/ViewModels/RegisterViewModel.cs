using System.ComponentModel.DataAnnotations;

namespace TechStore.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Please enter your full name")]
        [StringLength(100, ErrorMessage = "Full name must be between {2} and {1} characters", MinimumLength = 2)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your email")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your phone number")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your address")]
        [StringLength(100, ErrorMessage = "Address must be between {2} and {1} characters", MinimumLength = 5)]
        [Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your password")]
        [StringLength(100, ErrorMessage = "Password must be at least {2} characters long", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
} 