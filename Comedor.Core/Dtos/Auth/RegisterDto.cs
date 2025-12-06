using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Comedor.Core.Dtos.Auth;

public class RegisterDto
{
    [Required]
    [StringLength(64, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 64 caracteres.")]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress(ErrorMessage = "Email inválido.")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Teléfono inválido.")]
    public string? PhoneNumber { get; set; }

    // Nueva: lista de roles a asignar en la creación (opcional)
    public List<string> Roles { get; set; } = new();
}