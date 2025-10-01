using Microsoft.EntityFrameworkCore;

namespace Comedor.Core.Entities;

[Keyless]
public class ColaboradorView
{
    public string Identificacion { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string CentroCosto { get; set; } = string.Empty;
    public string? Observacion { get; set; }
}
