using Comedor.Core.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Comedor.Core.Interfaces;

public interface ICargoService
{
    Task<IEnumerable<CargoDto>> GetAllCargosAsync();
}
