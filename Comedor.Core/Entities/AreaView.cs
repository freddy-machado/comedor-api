using Microsoft.EntityFrameworkCore;

namespace Comedor.Core.Entities;

[Keyless]
public class AreaView
{
    public int Id { get; set; }
    public string Area { get; set; } = string.Empty;
}
