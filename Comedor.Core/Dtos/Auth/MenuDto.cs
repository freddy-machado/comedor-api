namespace Comedor.Core.Dtos.Auth;

public class MenuDto
{
    public int Id { get; set; }
    public string Key { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Href { get; set; }
    public List<ActionDto> AllowedActions { get; set; } = new();
    public List<MenuDto> Children { get; set; } = new();
}