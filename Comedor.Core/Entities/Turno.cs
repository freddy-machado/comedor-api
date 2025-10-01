
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Comedor.Core.Entities;

[Table("CM_Turnos")]
public class Turno
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column(TypeName = "decimal(18, 0)")]
    public decimal Id { get; set; }

    [StringLength(10)]
    public string? Desde { get; set; }

    [StringLength(10)]
    public string? Hasta { get; set; }

    [StringLength(30)]
    public string? TiempoComida { get; set; }

    [Column(TypeName = "decimal(12, 0)")]
    public decimal? RepetirSonidoNOK { get; set; }

    [Column(TypeName = "decimal(12, 0)")]
    public decimal? RepetirSonidoOK { get; set; }
}
