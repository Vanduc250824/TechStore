using System.ComponentModel.DataAnnotations;

public class RegisterViewModel
{
    [Required]
    public required string FullName { get; set; }

    [Required]
    public required string Username { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public required string Password { get; set; }

    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    public required string Phone { get; set; }
    public required string Address { get; set; }
}
