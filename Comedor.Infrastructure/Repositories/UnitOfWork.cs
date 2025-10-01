using Comedor.Core.Entities;
using Comedor.Core.Interfaces;
using Comedor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Comedor.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ComedorDbContext _context;
    public IComensalRepository Comensales { get; private set; }
    public IGenericRepository<Turno> Turnos { get; private set; }
    public IDespachoRepository Despachos { get; private set; }
    public IGenericRepository<Log> Logs { get; private set; }

    public UnitOfWork(ComedorDbContext context)
    {
        _context = context;
        Comensales = new ComensalRepository(_context);
        Turnos = new GenericRepository<Turno>(_context);
        Despachos = new DespachoRepository(_context);
        Logs = new GenericRepository<Log>(_context);
    }

    public async Task<int> SaveAsync()
    {
        string currentUser = "System"; // Placeholder for current user
        List<Log> logsToSave = new List<Log>(); // Temporary list for logs

        foreach (var entry in _context.ChangeTracker.Entries())
        {
            if (entry.Entity is Log || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            var log = new Log
            {
                Fecha = DateTime.UtcNow,
                Login = currentUser,
                Tabla = entry.Entity.GetType().Name,
                IdRegistro = GetEntityId(entry.Entity),
            };

            switch (entry.State)
            {
                case EntityState.Added:
                    log.Operacion = "Insert";
                    break;
                case EntityState.Modified:
                    log.Operacion = "Update";
                    break;
                case EntityState.Deleted:
                    log.Operacion = "Delete";
                    break;
            }
            logsToSave.Add(log); // Add to temporary list
        }

        // Add all collected logs to the context after the enumeration is complete
        if (logsToSave.Any())
        {
            await Logs.AddRangeAsync(logsToSave);
        }

        return await _context.SaveChangesAsync();
    }

    private int GetEntityId(object entity)
    {
        var idProperty = entity.GetType().GetProperty("Id");
        if (idProperty != null && idProperty.PropertyType == typeof(int))
        {
            return (int)idProperty.GetValue(entity);
        }
        if (idProperty != null && idProperty.PropertyType == typeof(decimal))
        {
            return (int)(decimal)idProperty.GetValue(entity); 
        }
        return 0; 
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}