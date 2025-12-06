
using System.Collections.Generic;

namespace Comedor.Core.Entities;

public class Menu
{
    public int Id { get; set; }
    public string Key { get; set; } = null!;    // Ej: "users"
    public string Title { get; set; } = null!;
    public string? Href { get; set; }
    public int? ParentId { get; set; }

    // Navegación jerárquica
    public Menu? Parent { get; set; }
    public ICollection<Menu>? Children { get; set; }

    // Relaciones a permisos
    public ICollection<RoleMenuAction>? RoleMenuActions { get; set; }
}