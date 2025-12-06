
namespace Comedor.Core.Entities;

public class RoleMenuAction
{
    // RoleId guarda el Id del IdentityRole (string). No dependemos del tipo IdentityRole desde Core.
    public string RoleId { get; set; } = null!;
    public int MenuId { get; set; }
    public int ActionId { get; set; }

    public Menu? Menu { get; set; }
    public MenuAction? Action { get; set; }
}