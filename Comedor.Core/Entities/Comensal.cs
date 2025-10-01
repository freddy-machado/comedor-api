using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Comedor.Core.Entities;

[Table("CM_Comensales")]
public class Comensal
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [StringLength(50)]
    public string? Identificacion { get; set; }

    [StringLength(150)]
    public string? NombreColaborador { get; set; }

    [StringLength(100)]
    public string? Area { get; set; }

    [StringLength(20)]
    public string? CentroCosto { get; set; }

    public int? TipoColaborador { get; set; }

    [StringLength(200)]
    public string? Observacion { get; set; }

    public DateTime? FechaDesde { get; set; }

    public DateTime? FechaHasta { get; set; }

    public bool? Empacadas { get; set; }

    [Column(TypeName = "numeric(12, 0)")]
    public decimal? CantidadComidas { get; set; }

    public bool? Activo { get; set; }

    public bool? Alerta { get; set; }

    [StringLength(100)]
    public string? CorreoAsunto { get; set; }

    [StringLength(200)]
    public string? CorreoPara { get; set; }

    [StringLength(200)]
    public string? CorreoDe { get; set; }

    [StringLength(200)]
    public string? CorreoCC { get; set; }
}
