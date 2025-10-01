using System;
using System.ComponentModel.DataAnnotations;

namespace Comedor.Core.Dtos;

public class UpdateDespachoDto
{
    [Required]
    public int Id { get; set; } // Required for update

    [Required]
    public decimal IdTurno { get; set; } // Required, matches Turno.Id type

    [Required]
    public int IdComensal { get; set; } // Required

    public DateTime Fecha { get; set; } // No default, will be provided

    [Required]
    [StringLength(50)]
    public string Identificacion { get; set; } = string.Empty;

    [Required]
    public decimal Cantidad { get; set; }

    public bool Activo { get; set; } // Will be provided

    public decimal CostoUnitario { get; set; }
}
