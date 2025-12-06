
namespace Comedor.Core.Entities;

public class MenuAction
{
    public int Id { get; set; }
    public string Key { get; set; } = null!;    // Ej: "view","edit","delete"
    public string Title { get; set; } = null!;

    public ICollection<RoleMenuAction>? RoleMenuActions { get; set; }
}