using Comedor.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Comedor.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Protect this controller
    public class ReportesController : ControllerBase
    {
        private readonly IReportesService _reportesService;

        public ReportesController(IReportesService reportesService)
        {
            _reportesService = reportesService;
        }

        [HttpGet("despachos")]
        public async Task<IActionResult> GetDespachosReport([FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin,
            string? search, int page, int pageSize)
        {
            var report = await _reportesService.GetDespachosReportAsync(fechaInicio, fechaFin, search, page, pageSize);
            return Ok(report);
        }
    }
}
