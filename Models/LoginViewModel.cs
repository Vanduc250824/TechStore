using System.ComponentModel.DataAnnotations;

namespace TechStore.Models
{
    public class LoginViewModel
    {
        [Required]
        public required string Email { get; set; }

        [Required]
        public required string Password { get; set; }
    }

}
