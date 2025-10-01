using System.ComponentModel.DataAnnotations;

namespace Comedor.Core.Dtos;

public class UpdateDespachoStatusDto
{
    [Required]
    public int Id { get; set; }
}
