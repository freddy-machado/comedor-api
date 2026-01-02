using Comedor.Core.Dtos;
using Comedor.Core.Dtos.Report;
using Comedor.Core.Entities.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comedor.Core.Interfaces
{
    public interface IReportesService
    {
        Task<PagedResultDto<DespachoReportDto>> GetDespachosReportAsync(DateTime fechaInicio, DateTime fechaFin,
            string? search, int page, int pageSize);
    }
}
