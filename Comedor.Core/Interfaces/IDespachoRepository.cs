using Comedor.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Comedor.Core.Interfaces;

public interface IDespachoRepository : IGenericRepository<Despacho>
{
    Task<IEnumerable<Despacho>> GetPendientesAsync();
    Task<IEnumerable<Despacho>> GetAllDespachosWithDetailsAsync();
    Task<(IEnumerable<Despacho> Items, int TotalCount)> GetPagedAsync(string? search, int page, int pageSize);
    // Add specific methods for Despacho if needed
}
