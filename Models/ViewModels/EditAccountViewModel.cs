using System.ComponentModel.DataAnnotations;

namespace TechStore.Models.ViewModels
{
    public class EditAccountViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter username")]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter email")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter phone number")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter full name")]
        [StringLength(100, ErrorMessage = "Full name must be between {2} and {1} characters", MinimumLength = 2)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter address")]
        [StringLength(100, ErrorMessage = "Address must be between {2} and {1} characters", MinimumLength = 5)]
        [Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;

        [Display(Name = "Role")]
        public string SelectedRole { get; set; } = string.Empty;

        public List<string> AvailableRoles { get; set; } = new List<string>();
    }
} 