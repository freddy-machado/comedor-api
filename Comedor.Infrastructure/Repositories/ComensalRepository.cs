using Comedor.Core.Entities;
using Comedor.Core.Interfaces;
using Comedor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Comedor.Infrastructure.Repositories;

public class ComensalRepository : GenericRepository<Comensal>, IComensalRepository
{
    public ComensalRepository(ComedorDbContext context) : base(context)
    {
    }

    public async Task<(IEnumerable<Comensal> Items, int TotalCount)> GetPagedAsync(string? search, int page, int pageSize)
    {
        var query = _context.Comensales.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var lowerCaseSearch = search.ToLower();
            query = query.Where(c => 
                (c.NombreColaborador != null && c.NombreColaborador.ToLower().Contains(lowerCaseSearch)) ||
                (c.Identificacion != null && c.Identificacion.ToLower().Contains(lowerCaseSearch)) ||
                (c.Area != null && c.Area.ToLower().Contains(lowerCaseSearch))
            );
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Comensal?> GetByIdentificacionAsync(string identificacion)
    {
        return await _context.Comensales.FirstOrDefaultAsync(c => c.Identificacion == identificacion);
    }

    public async Task<IEnumerable<Comensal>> GetAllActiveAsync()
    {
        return await _context.Comensales.Where(c => c.Activo == true).ToListAsync();
    }
}
