using System.ComponentModel.DataAnnotations;

namespace Comedor.Core.Dtos.Auth;

public class CreateUserDto
{
    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
