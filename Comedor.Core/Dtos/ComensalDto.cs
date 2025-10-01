namespace Comedor.Core.Dtos;

public class ComensalDto
{
    public int Id { get; set; }
    public string? Identificacion { get; set; }
    public string? NombreColaborador { get; set; }
    public string? Area { get; set; }
    public string? CentroCosto { get; set; }
    public string? Observacion { get; set; }
    public bool Activo { get; set; }
}
