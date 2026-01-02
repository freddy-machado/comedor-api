using Comedor.Core.Entities.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comedor.Core.Interfaces
{
    public interface IReportesRepository
    {
        Task<(IEnumerable<DespachoReport> lista, int TotalCount)> GetDespachosReportAsync(DateTime fechaInicio, DateTime fechaFin,
            string? search, int page, int pageSize);
    }
}
