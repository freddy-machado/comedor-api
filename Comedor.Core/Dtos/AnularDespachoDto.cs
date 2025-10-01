using System.ComponentModel.DataAnnotations;

namespace Comedor.Core.Dtos;

public class AnularDespachoDto
{
    [Required]
    public int Id { get; set; }
}
