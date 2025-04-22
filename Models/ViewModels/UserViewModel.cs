using System.ComponentModel.DataAnnotations;

namespace TechStore.Models.ViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Username is required")]
        public string UserName { get; set; }

        public List<string> Roles { get; set; } = new List<string>();
        public List<Microsoft.AspNetCore.Identity.IdentityRole> AllRoles { get; set; } = new List<Microsoft.AspNetCore.Identity.IdentityRole>();
    }
} 