using Comedor.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Comedor.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // Protect this controller
public class CargosController : ControllerBase
{
    private readonly ICargoService _cargoService;

    public CargosController(ICargoService cargoService)
    {
        _cargoService = cargoService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCargos()
    {
        var cargos = await _cargoService.GetAllCargosAsync();
        return Ok(cargos);
    }
}
