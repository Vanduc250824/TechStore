using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TechStore.Models;

namespace TechStore.Models
{
    public class EditUserView
    {
        [Key]
        public required string Id { get; set; }

        [Required]
        public required string UserName { get; set; }

        [Required]
        public required string FullName { get; set; }

        [Required]
        public required string Email { get; set; }

        [Required]
        public required string Address { get; set; }

        public string? ProfilePicture { get; set; }
    }

}
