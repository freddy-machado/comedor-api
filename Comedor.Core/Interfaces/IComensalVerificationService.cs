using Comedor.Core.Dtos;
using System.Threading.Tasks;

namespace Comedor.Core.Interfaces;

public interface IComensalVerificationService
{
    Task<(ComensalDto? comensal, string? errorMessage)> VerifyAndRegisterComensalAsync(string identificacion);
}
