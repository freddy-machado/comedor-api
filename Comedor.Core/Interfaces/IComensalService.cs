using Comedor.Core.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Comedor.Core.Interfaces
{
    public interface IComensalService
    {
        Task<PagedResultDto<ComensalDto>> GetComensalesAsync(string? search, int page, int pageSize);
        Task<ComensalDto> GetComensalByIdAsync(int id);
        Task<ComensalCreateDto> GetComensalCreateDtoByIdAsync(int id);
        Task<ComensalDto?> UpsertComensalAsync(ComensalCreateDto dto); // Método agregado
        Task<bool> InactivateComensalAsync(int id);
        Task<IEnumerable<ComensalDto>> GetAllActiveComensalesAsync();
    }
}
