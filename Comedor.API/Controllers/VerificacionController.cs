using Comedor.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Comedor.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // Protect this controller
public class VerificacionController : ControllerBase
{
    private readonly IComensalVerificationService _verificationService;

    public VerificacionController(IComensalVerificationService verificationService)
    {
        _verificationService = verificationService;
    }

    [HttpGet("{identificacion}")]
    public async Task<IActionResult> Verificar(string identificacion)
    {
        if (string.IsNullOrEmpty(identificacion))
        {
            return BadRequest(new { Message = "El número de identificación no puede estar vacío." });
        }

        var (comensal, errorMessage) = await _verificationService.VerifyAndRegisterComensalAsync(identificacion);

        if (comensal == null)
        {
            // Use the specific error message from the service, or a generic one if null
            return NotFound(new { Message = errorMessage ?? $"Colaborador con identificación '{identificacion}' no fue encontrado o no es apto para consumir." });
        }

        return Ok(comensal);
    }
}
