using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Comedor.Core.Dtos.Auth;

public partial class UserDto
{
    public string Id { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string UserNameApplication { get; set; } = null!;
    // Access token (JWT)
    public string? Token { get; set; }
    public bool? IsAuthenticated { get; set; }
    public string? Message { get; set; }
    public DateTime PasswordExpiration { get; set; }
    public List<string> Roles { get; set; } = new();
    public List<ClaimDto> Claims { get; set; } = new();

    // Exponemos el refresh token al cliente para que el cliente lo guarde de forma segura
    // (si prefieres no enviarlo aquí, mantén [JsonIgnore] y el cliente debe llamar a /users/{id}/refresh/generate)
    //[JsonIgnore]
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiration { get; set; }
}