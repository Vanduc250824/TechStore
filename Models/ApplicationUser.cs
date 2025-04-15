using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
public class ApplicationUser : IdentityUser
{
    public required string FullName { get; set; }
    public required string Address { get; set; }
    public string? ProfilePicture { get; set; }
}
