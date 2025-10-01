using System.ComponentModel.DataAnnotations;

namespace Comedor.Core.Dtos.Auth;

public class RegisterDto
{
    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; } = true;
}