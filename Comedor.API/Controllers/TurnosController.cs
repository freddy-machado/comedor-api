using Comedor.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Comedor.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TurnosController : ControllerBase
    {
        private readonly ITurnoService _turnoService;

        public TurnosController(ITurnoService turnoService)
        {
            _turnoService = turnoService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTurnos()
        {
            var turnos = await _turnoService.GetAllTurnosAsync();
            return Ok(turnos);
        }
    }
}
