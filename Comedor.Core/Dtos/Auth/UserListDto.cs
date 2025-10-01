namespace Comedor.Core.Dtos.Auth;

public class UserListDto
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string NormalizedUserName { get; set; } = string.Empty; // New field
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}