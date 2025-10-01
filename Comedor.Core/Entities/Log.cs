using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Comedor.Core.Entities;

[Table("CM_Log")]
public class Log
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int IdRegistro { get; set; }

    [StringLength(256)]
    public string? Login { get; set; }

    public DateTime? Fecha { get; set; }

    [StringLength(50)]
    public string? Tabla { get; set; }

    [StringLength(30)]
    public string? Operacion { get; set; }
}
