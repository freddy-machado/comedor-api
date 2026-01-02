using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comedor.Core.Entities.Custom
{
    public class DespachoReport
    {
        public DateTime? Fecha { get; set; }

        public string? HoraDespacho { get; set; } = string.Empty;

        public string? Identificacion { get; set; } = string.Empty;

        public string? NombreColaborador { get; set; } = string.Empty;

        public string? Origin { get; set; } = string.Empty;

        public string? Area { get; set; } = string.Empty;

        // Se deja como string para conservar formatos, ceros a la izquierda o prefijos no numéricos
        public string? CentroCosto { get; set; } = string.Empty;

        public string? TiempoComida { get; set; } = string.Empty;
    }
}
