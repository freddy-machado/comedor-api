using System;

namespace Comedor.Core.Dtos;

public class DespachoDto
{
    public int Id { get; set; }
    public decimal? IdTurno { get; set; }
    public int? IdComensal { get; set; }
    public DateTime? Fecha { get; set; }
    public string? Identificacion { get; set; }
    public decimal? Cantidad { get; set; }
    public bool? Activo { get; set; }
    public decimal? CostoUnitario { get; set; }
    
    // New fields
    public bool? Despachado { get; set; }
    public string? HoraDespacho { get; set; }

    // Added for list display
    public string NombreColaborador { get; set; } = string.Empty;
    public string TurnoTiempo { get; set; } = string.Empty;
    public string TiempoComida { get; set; } = string.Empty;
}