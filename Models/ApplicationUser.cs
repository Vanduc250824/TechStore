using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
public class ApplicationUser : IdentityUser
{
    [Required]
    public required string FullName { get; set; }
    [Required]
    public required string Address { get; set; }

    public string ProfilePicture { get; set; } 
}
