using Microsoft.EntityFrameworkCore;

namespace Comedor.Core.Entities;

[Keyless]
public class CargoView
{
    public int Id { get; set; }
    public string Cargo { get; set; } = string.Empty;
}
