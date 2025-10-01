using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Comedor.Core.Enums; // Added for DespachoOrigin

namespace Comedor.Core.Entities;

[Table("CM_Despachos")]
public class Despacho
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    // FK to Turno
    public decimal? IdTurno { get; set; }

    // FK to Comensal
    public int? IdComensal { get; set; }

    public DateTime? Fecha { get; set; }

    [StringLength(50)]
    public string? Identificacion { get; set; }

    [Column(TypeName = "numeric(12, 0)")]
    public decimal? Cantidad { get; set; }

    public bool? Activo { get; set; }

    [Column(TypeName = "numeric(12, 2)")]
    public decimal? CostoUnitario { get; set; }

    // New fields
    public bool? Despachado { get; set; }
    
    [StringLength(15)]
    public string? HoraDespacho { get; set; }

    public DespachoOrigin Origin { get; set; } // New field

    // Navigation properties
    [ForeignKey("IdTurno")]
    public virtual Turno? Turno { get; set; }

    [ForeignKey("IdComensal")]
    public virtual Comensal? Comensal { get; set; }
}
