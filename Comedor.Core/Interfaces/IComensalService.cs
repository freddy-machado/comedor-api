using Comedor.Core.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Comedor.Core.Interfaces
{
    public interface IComensalService
    {
        Task<PagedResultDto<ComensalDto>> GetComensalesAsync(string? search, int page, int pageSize);
        Task<ComensalDto> GetComensalByIdAsync(int id);
        Task<ComensalDto> CreateComensalAsync(ComensalCreateDto createDto);
        Task<bool> UpdateComensalAsync(int id, ComensalCreateDto updateDto);
        Task<bool> InactivateComensalAsync(int id);
        Task<IEnumerable<ComensalDto>> GetAllActiveComensalesAsync();
    }
}
