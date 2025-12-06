using Comedor.Core.Dtos;
using Comedor.Core.Dtos.Auth;
using Comedor.Core.Entities;
using Comedor.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Comedor.Infrastructure.Services;

public class PermissionService
{
    private readonly ComedorDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public PermissionService(ComedorDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IEnumerable<MenuDto>> GetMenuForUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return Enumerable.Empty<MenuDto>();

        var roles = await _userManager.GetRolesAsync(user);

        // Obtener asignaciones permitidas por roles
        var permitted = await (from rma in _db.RoleMenuActions.AsNoTracking()
                               join m in _db.Menus on rma.MenuId equals m.Id
                               join a in _db.MenuActions on rma.ActionId equals a.Id
                               join r in _db.Roles on rma.RoleId equals r.Id
                               where roles.Contains(r.Name)
                               select new { m.Id, m.Key, m.Title, m.Href, m.ParentId, ActionKey = a.Key })
                              .ToListAsync();

        if (!permitted.Any()) return Enumerable.Empty<MenuDto>();

        // Agrupar por menu y coleccionar acciones (como ActionDto)
        var grouped = permitted
            .GroupBy(p => new { p.Id, p.Key, p.Title, p.Href, p.ParentId })
            .Select(g => new
            {
                Menu = new MenuDto { Id = g.Key.Id, Key = g.Key.Key, Title = g.Key.Title, Href = g.Key.Href },
                ParentId = g.Key.ParentId,
                Actions = g
                    .Select(x => new ActionDto { Key = x.ActionKey, Title = x.ActionKey })
                    .GroupBy(a => a.Key)
                    .Select(ga => ga.First())
                    .ToList()
            })
            .ToList();

        // Construir diccionario de menus permitidos
        var dict = grouped.ToDictionary(x => x.Menu.Id, x =>
        {
            x.Menu.AllowedActions.AddRange(x.Actions);
            return x.Menu;
        });

        // Asegurar que padres también aparezcan (sin acciones si no tienen)
        var menuIds = dict.Keys.ToHashSet();
        var allMenus = await _db.Menus.AsNoTracking().Where(m => menuIds.Contains(m.Id) || menuIds.Contains(m.ParentId ?? -1)).ToListAsync();

        // Añadir padres que falten
        foreach (var m in allMenus)
        {
            if (!dict.ContainsKey(m.Id))
            {
                dict[m.Id] = new MenuDto { Id = m.Id, Key = m.Key, Title = m.Title, Href = m.Href };
            }
        }

        // Construir árbol
        foreach (var menu in dict.Values)
        {
            var entity = allMenus.FirstOrDefault(x => x.Id == menu.Id);
            if (entity != null && entity.ParentId.HasValue && dict.TryGetValue(entity.ParentId.Value, out var parent))
            {
                parent.Children.Add(menu);
            }
        }

        // Devolver sólo raíces (ParentId == null)
        var roots = allMenus.Where(m => m.ParentId == null).Select(m => dict[m.Id]).ToList();
        return roots;
    }
}