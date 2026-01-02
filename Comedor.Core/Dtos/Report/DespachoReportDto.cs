using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comedor.Core.Dtos.Report
{
    public class DespachoReportDto
    {
        public DateTime? Fecha { get; set; }

        public string? HoraDespacho { get; set; }

        public string? Identificacion { get; set; } = string.Empty;

        public string? NombreColaborador { get; set; } = string.Empty;

        public string? Origin { get; set; } = string.Empty;

        public string? Area { get; set; } = string.Empty;

        public string? CentroCosto { get; set; } = string.Empty;

        public string? TiempoComida { get; set; } = string.Empty;
    }
}
