using Comedor.Core.Dtos.Report;
using Comedor.Core.Entities.Custom;
using Comedor.Core.Interfaces;
using Comedor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Comedor.Infrastructure.Repositories
{
    public class ReportesRepository: GenericRepository<DespachoReport>, IReportesRepository
    {        
        private const string StoredProcedureName = "sp_DespachosComedor"; 

        public ReportesRepository(ComedorDbContext context): base(context)
        {            
        }
        
        public async Task<(IEnumerable<DespachoReport> lista, int TotalCount) > GetDespachosReportAsync(DateTime fechaInicio, DateTime fechaFin, 
            string? search, int page, int pageSize)
        {
            // Formato ISO requerido por el SP (varchar(10))
            var inicio = fechaInicio.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var fin = fechaFin.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            // FormattableString permite que EF Core cree parámetros seguros
            var query = _context.Database
                .SqlQuery<DespachoReport>($"EXEC {StoredProcedureName} @FechaInicial={inicio}, @FechaFinal={fin}");

            // Ejecuta y materializa
            var results = await query.ToListAsync();


            // Normalizar parámetros de paginación
            page = Math.Max(1, page);
            pageSize = Math.Max(1, pageSize);

            // Filtrado en memoria similar al ejemplo provisto
            IEnumerable<DespachoReport> filtered = results;

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lowerCaseSearch = search.ToLowerInvariant();
                filtered = filtered.Where(d =>
                {
                    var composite = string.Concat(
                        (d.Identificacion ?? string.Empty), " ",
                        (d.NombreColaborador ?? string.Empty), " ",
                        (d.Area ?? string.Empty), " ",
                        (d.CentroCosto ?? string.Empty), " ",
                        (d.TiempoComida ?? string.Empty), " ",
                        (d.Origin ?? string.Empty)
                    );

                    return composite.ToLowerInvariant().Contains(lowerCaseSearch);
                });
            }

            // Total si necesita (no se devuelve actualmente, pero se calcula si desea usar)
            var totalCount = filtered.Count();

            var paged = filtered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (paged, totalCount) ;

            //return results;
        }        
    }
}
