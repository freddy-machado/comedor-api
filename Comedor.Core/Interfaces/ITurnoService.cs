using Comedor.Core.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Comedor.Core.Interfaces
{
    public interface ITurnoService
    {
        Task<IEnumerable<TurnoDto>> GetAllTurnosAsync();
    }
}
