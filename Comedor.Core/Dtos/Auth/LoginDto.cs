using System.ComponentModel.DataAnnotations;

namespace Comedor.Core.Dtos.Auth;

public class LoginDto
{
    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}