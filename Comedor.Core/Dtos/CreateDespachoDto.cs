using System;
using System.ComponentModel.DataAnnotations;
using Comedor.Core.Enums; // Added for DespachoOrigin

namespace Comedor.Core.Dtos;

public class CreateDespachoDto
{
    [Required]
    public decimal IdTurno { get; set; } // Required, matches Turno.Id type

    [Required]
    public int IdComensal { get; set; } // Required

    public DateTime Fecha { get; set; } = DateTime.UtcNow; // Default to now

    [Required]
    [StringLength(50)]
    public string Identificacion { get; set; } = string.Empty;

    [Required]
    public decimal Cantidad { get; set; }

    public bool Activo { get; set; } = true; // Default to active

    public decimal CostoUnitario { get; set; }

    public DespachoOrigin Origin { get; set; } // New field
}