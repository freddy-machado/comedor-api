using Comedor.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Comedor.Infrastructure.Data;

public class ComedorDbContext : IdentityDbContext<ApplicationUser>
{
    public ComedorDbContext(DbContextOptions<ComedorDbContext> options) : base(options)
    {
    }

    public DbSet<Comensal> Comensales { get; set; }
    public DbSet<Turno> Turnos { get; set; }
    public DbSet<Despacho> Despachos { get; set; }
    public DbSet<Log> Logs { get; set; }

    public DbSet<ColaboradorView> ColaboradorViews { get; set; }
    public DbSet<AreaView> AreaViews { get; set; }
    public DbSet<CargoView> CargoViews { get; set; }

    // Nuevos DbSets para menú/acciones/roles
    public DbSet<Menu> Menus { get; set; }
    public DbSet<MenuAction> MenuActions { get; set; }
    public DbSet<RoleMenuAction> RoleMenuActions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Valores por defecto existentes
        builder.Entity<Turno>()
            .Property(t => t.RepetirSonidoNOK)
            .HasDefaultValue(1M);

        builder.Entity<Turno>()
            .Property(t => t.RepetirSonidoOK)
            .HasDefaultValue(1M);

        // Keyless views (read-only)
        builder.Entity<ColaboradorView>().HasNoKey();
        builder.Entity<AreaView>().HasNoKey();
        builder.Entity<CargoView>().HasNoKey();

        // Menú jerárquico (evitar borrado en cascada que rompa árbol)
        builder.Entity<Menu>()
            .HasMany(m => m.Children)
            .WithOne(m => m.Parent)
            .HasForeignKey(m => m.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        // RoleMenuAction: PK compuesta y FKs
        builder.Entity<RoleMenuAction>()
            .HasKey(rma => new { rma.RoleId, rma.MenuId, rma.ActionId });

        builder.Entity<RoleMenuAction>()
            .HasOne(rma => rma.Menu)
            .WithMany(m => m.RoleMenuActions)
            .HasForeignKey(rma => rma.MenuId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<RoleMenuAction>()
            .HasOne(rma => rma.Action)
            .WithMany(a => a.RoleMenuActions)
            .HasForeignKey(rma => rma.ActionId)
            .OnDelete(DeleteBehavior.Cascade);

        // FK hacia AspNetRoles (sin navegación para no acoplar Core a Identity)
        builder.Entity<RoleMenuAction>()
            .HasOne<IdentityRole>()
            .WithMany()
            .HasForeignKey(rma => rma.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índice para búsquedas por role
        builder.Entity<RoleMenuAction>()
            .HasIndex(r => r.RoleId);
    }
}