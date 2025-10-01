using System;

namespace Comedor.Core.Dtos
{
    public class ComensalCreateDto
    {
        public string? Identificacion { get; set; }
        public string? NombreColaborador { get; set; }
        public string? Area { get; set; }
        public string? CentroCosto { get; set; }
        public int? TipoColaborador { get; set; }
        public string? Observacion { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public bool? Empacadas { get; set; }
        public decimal? CantidadComidas { get; set; }
        public bool? Activo { get; set; }
        public bool? Alerta { get; set; }
        public string? CorreoAsunto { get; set; }
        public string? CorreoPara { get; set; }
        public string? CorreoDe { get; set; }
        public string? CorreoCC { get; set; }
    }
}
