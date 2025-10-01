using System;

namespace Comedor.Core.Dtos;

public class DespachoNotificationDto
{
    public int Id { get; set; }
    public string Identificacion { get; set; } = string.Empty;
    public string NombreColaborador { get; set; } = string.Empty;
    public string TurnoTiempo { get; set; } = string.Empty; // e.g., "08:00 - 16:00"
    public DateTime FechaRegistro { get; set; }
    public string HoraDespacho { get; set; } = string.Empty; // Will be null initially, then updated
    public bool? Despachado { get; set; } // Added field
}
