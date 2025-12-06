using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Comedor.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Comedor.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var db = scope.ServiceProvider.GetRequiredService<ComedorDbContext>();

        // Roles base (asegura que existan)
        var roles = new[] { "Admin", "Viewer", "Despacho", "Registro", "User" };
        foreach (var r in roles)
        {
            if (!await roleManager.RoleExistsAsync(r))
                await roleManager.CreateAsync(new IdentityRole(r));
        }

        // Acciones
        if (!db.MenuActions.Any())
        {
            db.MenuActions.AddRange(new MenuAction { Key = "new", Title = "Nuevo" },
                                    new MenuAction { Key = "view", Title = "Ver" },
                                    new MenuAction { Key = "edit", Title = "Editar" },
                                    new MenuAction { Key = "delete", Title = "Eliminar" },
                                    new MenuAction { Key = "activate", Title = "Activar" },
                                    new MenuAction { Key = "deactivate", Title = "Inactivar" });
            await db.SaveChangesAsync();
        }

        // Menús (crea solo si no existen)
        if (!db.Menus.Any(m => m.Href == "registro"))
            db.Menus.Add(new Menu { Key = "registro", Title = "Registrarse", Href = "registro" });

        if (!db.Menus.Any(m => m.Href == "despachos"))
            db.Menus.Add(new Menu { Key = "despachos", Title = "Despacho", Href = "despachos" });

        if (!db.Menus.Any(m => m.Href == "registros-despachos-r"))
            db.Menus.Add(new Menu { Key = "registros-despachos-r", Title = "Registro", Href = "registros-despachos-r" });

        if (!db.Menus.Any(m => m.Href == "comensales"))
            db.Menus.Add(new Menu { Key = "comensales", Title = "Colaboradores", Href = "comensales" });

        if (!db.Menus.Any(m => m.Href == "usuarios"))
            db.Menus.Add(new Menu { Key = "usuarios", Title = "Lista de Usuarios", Href = "usuarios" });

        if (!db.Menus.Any(m => m.Href == "roles"))
            db.Menus.Add(new Menu { Key = "roles", Title = "Lista de Roles", Href = "roles" });

        await db.SaveChangesAsync();

        // IDs de acciones
        var viewId = db.MenuActions.Single(a => a.Key == "view").Id;
        var editId = db.MenuActions.Single(a => a.Key == "edit").Id;
        var deleteId = db.MenuActions.Single(a => a.Key == "delete").Id;
        var newId = db.MenuActions.Single(a => a.Key == "new").Id;
        var activateId = db.MenuActions.Single(a => a.Key == "activate").Id;
        var deactivateId = db.MenuActions.Single(a => a.Key == "deactivate").Id;

        // Obtener menús creados
        var menuRegistro = db.Menus.Single(m => m.Href == "registro");
        var menuDespachos = db.Menus.Single(m => m.Href == "despachos");
        var menuRegistrosDespachosR = db.Menus.Single(m => m.Href == "registros-despachos-r");
        var menuComensales = db.Menus.Single(m => m.Href == "comensales");
        var menuUsuarios = db.Menus.Single(m => m.Href == "usuarios");
        var menuRoles = db.Menus.Single(m => m.Href == "roles");

        // Obtener roles
        string adminRoleId = roleManager.Roles.Single(r => r.Name == "Admin").Id;
        string viewerRoleId = roleManager.Roles.Single(r => r.Name == "Viewer").Id;
        string despachoRoleId = roleManager.Roles.Single(r => r.Name == "Despacho").Id;
        string registroRoleId = roleManager.Roles.Single(r => r.Name == "Registro").Id;

        // --- Deduplicación en memoria: leer existentes y planificar nuevas inserciones ---
        var existingKeys = db.RoleMenuActions
            .AsNoTracking()
            .Select(rma => new ValueTuple<string, int, int>(rma.RoleId, rma.MenuId, rma.ActionId))
            .ToHashSet();

        var plannedKeys = new HashSet<(string roleId, int menuId, int actionId)>();
        var additions = new List<RoleMenuAction>();

        void PlanIfNotExists(string roleId, int menuId, int actionId)
        {
            var key = (roleId, menuId, actionId);
            if (existingKeys.Contains(new ValueTuple<string, int, int>(roleId, menuId, actionId)))
                return;
            if (plannedKeys.Contains(key))
                return;

            plannedKeys.Add(key);
            additions.Add(new RoleMenuAction { RoleId = roleId, MenuId = menuId, ActionId = actionId });
        }

        // Admin: todos los menús, todas las acciones (CRUD + activar/inactivar)
        var allMenus = new[] { menuRegistro, menuDespachos, menuRegistrosDespachosR, menuComensales, menuUsuarios, menuRoles };
        var allActions = new[] { newId, viewId, editId, deleteId, activateId, deactivateId };
        foreach (var m in allMenus)
            foreach (var a in allActions)
                PlanIfNotExists(adminRoleId, m.Id, a);

        // Viewer: solo ver en todos los menús
        foreach (var m in allMenus)
            PlanIfNotExists(viewerRoleId, m.Id, viewId);

        // Despacho: acceso solo al menú "despachos"
        foreach (var a in new[] { newId, viewId, editId, deleteId })
            PlanIfNotExists(despachoRoleId, menuDespachos.Id, a);

        // Registro: acceso solo al menú "registro"
        foreach (var a in new[] { newId, viewId, editId, deleteId })
            PlanIfNotExists(registroRoleId, menuRegistro.Id, a);

        // Mapear botones específicos a acciones en menús concretos:
        PlanIfNotExists(adminRoleId, menuComensales.Id, newId);
        PlanIfNotExists(adminRoleId, menuComensales.Id, editId);
        PlanIfNotExists(adminRoleId, menuComensales.Id, deactivateId);
        PlanIfNotExists(viewerRoleId, menuComensales.Id, viewId);

        PlanIfNotExists(adminRoleId, menuRegistrosDespachosR.Id, newId);
        PlanIfNotExists(adminRoleId, menuRegistrosDespachosR.Id, editId);
        PlanIfNotExists(adminRoleId, menuRegistrosDespachosR.Id, deleteId);
        PlanIfNotExists(viewerRoleId, menuRegistrosDespachosR.Id, viewId);

        PlanIfNotExists(adminRoleId, menuUsuarios.Id, newId);
        PlanIfNotExists(adminRoleId, menuUsuarios.Id, editId);
        PlanIfNotExists(adminRoleId, menuUsuarios.Id, activateId);
        PlanIfNotExists(adminRoleId, menuUsuarios.Id, deactivateId);
        PlanIfNotExists(viewerRoleId, menuUsuarios.Id, viewId);

        // Añadir todo de una vez y un único SaveChanges
        if (additions.Count > 0)
        {
            db.RoleMenuActions.AddRange(additions);
            await db.SaveChangesAsync();
        }
    }
}