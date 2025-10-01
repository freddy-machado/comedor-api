using Comedor.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Comedor.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // Protect this controller
public class AreasController : ControllerBase
{
    private readonly IAreaService _areaService;

    public AreasController(IAreaService areaService)
    {
        _areaService = areaService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAreas()
    {
        var areas = await _areaService.GetAllAreasAsync();
        return Ok(areas);
    }
}
