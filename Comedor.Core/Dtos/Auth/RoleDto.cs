using System.ComponentModel.DataAnnotations;

namespace Comedor.Core.Dtos.Auth;

public class RoleDto
{
    [Required]
    public string Role { get; set; } = null!;
}