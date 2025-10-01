using Comedor.Core.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Comedor.Core.Interfaces;

public interface IDespachoService
{
    Task<IEnumerable<DespachoDto>> GetAllDespachosAsync();
    Task<PagedResultDto<DespachoDto>> GetDespachosPaginadosAsync(string? search, int page, int pageSize);
    Task<IEnumerable<DespachoNotificationDto>> GetDespachosPendientesAsync();
    Task<DespachoDto?> GetDespachoByIdAsync(int id);
    Task<DespachoDto> CreateDespachoAsync(CreateDespachoDto createDto);
    Task<DespachoDto?> UpdateDespachoAsync(UpdateDespachoDto updateDto);
    Task<bool> AnularDespachoAsync(AnularDespachoDto anularDto);
    Task<bool> DeleteDespachoAsync(int id);
    Task<DespachoDto?> UpdateDespachoStatusAsync(UpdateDespachoStatusDto updateStatusDto);
    
    // New method for verification process
    Task<DespachoDto> CreateDespachoFromVerificationAsync(int idComensal, string identificacion);
}
