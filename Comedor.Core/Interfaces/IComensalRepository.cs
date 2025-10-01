using Comedor.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Comedor.Core.Interfaces;

public interface IComensalRepository : IGenericRepository<Comensal>
{
    Task<(IEnumerable<Comensal> Items, int TotalCount)> GetPagedAsync(string? search, int page, int pageSize);
    Task<Comensal?> GetByIdentificacionAsync(string identificacion);
    Task<IEnumerable<Comensal>> GetAllActiveAsync();
}
