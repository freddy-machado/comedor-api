using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Comedor.Core.Dtos.Auth;

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserNameApplication { get; set; } = string.Empty;
    public string? Token { get; set; }
    public bool? IsAuthenticated { get; set; }
    public string? Message { get; set; }
    public DateTime PasswordExpiration { get; set; }
    public List<string> Roles { get; set; } = new List<string>();
    public List<ClaimDto> Claims { get; set; } = new List<ClaimDto>();

    [JsonIgnore]
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiration { get; set; }
}