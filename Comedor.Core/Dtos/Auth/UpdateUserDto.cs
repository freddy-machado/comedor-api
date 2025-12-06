using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

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

    public string NormalizedUserName { get; set; } = string.Empty;

    // Nueva: lista de roles a sincronizar durante la actualización (opcional)
    public List<string> Roles { get; set; } = new();

    // Nueva: contraseña opcional para actualizar (si está vacía o null, no se cambia)
    // Las validaciones (longitud, confirmación, fuerza) las hace el cliente según indicas.
    public string? Password { get; set; }
}
