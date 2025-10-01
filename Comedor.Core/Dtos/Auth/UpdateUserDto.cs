using System.ComponentModel.DataAnnotations;

namespace Comedor.Core.Dtos.Auth;

public class UpdateUserDto
{
    [Required]
    public string Id { get; set; } = string.Empty; // User ID is required for update

    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
}
