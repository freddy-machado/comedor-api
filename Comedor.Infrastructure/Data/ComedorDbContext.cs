using Comedor.Core.Entities;
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
    
    // Add DbSet for ColaboradorView
    public DbSet<ColaboradorView> ColaboradorViews { get; set; }

    // Add DbSets for AreaView and CargoView
    public DbSet<AreaView> AreaViews { get; set; }
    public DbSet<CargoView> CargoViews { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Set default values as per the DDL
        builder.Entity<Turno>()
            .Property(t => t.RepetirSonidoNOK)
            .HasDefaultValue(1M);
            
        builder.Entity<Turno>()
            .Property(t => t.RepetirSonidoOK)
            .HasDefaultValue(1M);

        // Configure ColaboradorView as a keyless entity
        builder.Entity<ColaboradorView>().HasNoKey();

        // Configure AreaView as a keyless entity
        builder.Entity<AreaView>().HasNoKey();

        // Configure CargoView as a keyless entity
        builder.Entity<CargoView>().HasNoKey();
    }
}