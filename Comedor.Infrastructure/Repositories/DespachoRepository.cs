using Comedor.Core.Entities;
using Comedor.Core.Interfaces;
using Comedor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Comedor.Infrastructure.Repositories;

public class DespachoRepository : GenericRepository<Despacho>, IDespachoRepository
{
    public DespachoRepository(ComedorDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Despacho>> GetPendientesAsync()
    {
        return await _context.Despachos
                             .Include(d => d.Comensal) // Include related Comensal
                             .Include(d => d.Turno)    // Include related Turno
                             .Where(d => d.Despachado == false)
                             .ToListAsync();
    }

    public async Task<IEnumerable<Despacho>> GetAllDespachosWithDetailsAsync()
    {
        return await _context.Despachos
                             .Include(d => d.Comensal)
                             .Include(d => d.Turno)
                             .OrderByDescending(d => d.Fecha)
                             .ToListAsync();
    }

    public async Task<(IEnumerable<Despacho> Items, int TotalCount)> GetPagedAsync(string? search, int page, int pageSize)
    {
        var query = _context.Despachos
                             .Include(d => d.Comensal)
                             .Include(d => d.Turno)
                             .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var lowerCaseSearch = search.ToLower();
            query = query.Where(d =>
                (d.Comensal.NombreColaborador != null && d.Comensal.NombreColaborador.ToLower().Contains(lowerCaseSearch)) ||
                (d.Comensal.Identificacion != null && d.Comensal.Identificacion.ToLower().Contains(lowerCaseSearch))
            );
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(d => d.Fecha)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    // Add specific methods for Despacho here if needed
}
