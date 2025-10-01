using Comedor.Core.Dtos;
using Comedor.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System; 
using System.Linq; 
using Comedor.Core.Enums; // Added for DespachoOrigin

namespace Comedor.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // Protect this controller
public class DespachosController : ControllerBase
{
    private readonly IDespachoService _despachoService; // Changed to specific service
    private readonly AutoMapper.IMapper _mapper; // Keep mapper for consistency, though service handles mapping

    public DespachosController(IDespachoService despachoService, AutoMapper.IMapper mapper) // Changed injection
    {
        _despachoService = despachoService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DespachoDto>>> GetAllDespachos()
    {
        var despachos = await _despachoService.GetAllDespachosAsync();
        return Ok(despachos);
    }

    [HttpGet("paginados")]
    public async Task<IActionResult> GetDespachosPaginados([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _despachoService.GetDespachosPaginadosAsync(search, page, pageSize);
        return Ok(result);
    }

    [HttpGet("pendientes")]
    public async Task<ActionResult<IEnumerable<DespachoNotificationDto>>> GetDespachosPendientes()
    {
        var despachos = await _despachoService.GetDespachosPendientesAsync();
        return Ok(despachos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DespachoDto>> GetDespachoById(int id)
    {
        var despacho = await _despachoService.GetDespachoByIdAsync(id);
        if (despacho == null)
        {
            return NotFound(new { Message = $"Despacho with ID {id} not found." });
        }
        return Ok(despacho);
    }

    [HttpPost]
    public async Task<IActionResult> CreateDespacho([FromBody] CreateDespachoDto createDto)
    {
        try
        {
            createDto.Origin = DespachoOrigin.Administration; // Set origin for manual creation
            var despacho = await _despachoService.CreateDespachoAsync(createDto);
            return CreatedAtAction(nameof(GetDespachoById), new { id = despacho.Id }, despacho);
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            // Log the exception
            return StatusCode(500, new { Message = "An error occurred while creating the despacho.", Details = ex.Message });
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateDespacho([FromBody] UpdateDespachoDto updateDto)
    {
        try
        {
            var despacho = await _despachoService.UpdateDespachoAsync(updateDto);
            if (despacho == null)
            {
                return NotFound(new { Message = $"Despacho with ID {updateDto.Id} not found." });
            }
            return Ok(despacho);
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            // Log the exception
            return StatusCode(500, new { Message = "An error occurred while updating the despacho.", Details = ex.Message });
        }
    }

    [HttpPatch("anular")] // Using HttpPatch for partial update/status change
    public async Task<IActionResult> AnularDespacho([FromBody] AnularDespachoDto anularDto)
    {
        try
        {
            var result = await _despachoService.AnularDespachoAsync(anularDto);
            if (!result)
            {
                return NotFound(new { Message = $"Despacho with ID {anularDto.Id} not found." });
            }
            return NoContent(); // 204 No Content is appropriate for successful update with no new resource
        }
        catch (Exception ex)
        {
            // Log the exception
            return StatusCode(500, new { Message = "An error occurred while anulling the despacho.", Details = ex.Message });
        }
    }

    [HttpPut("status")] // New endpoint for updating status
    public async Task<IActionResult> UpdateDespachoStatus([FromBody] UpdateDespachoStatusDto updateStatusDto)
    {
        try
        {
            var despacho = await _despachoService.UpdateDespachoStatusAsync(updateStatusDto);
            if (despacho == null)
            {
                return NotFound(new { Message = $"Despacho with ID {updateStatusDto.Id} not found." });
            }
            return Ok(despacho);
        }
        catch (Exception ex)
        {
            // Log the exception
            return StatusCode(500, new { Message = "An error occurred while updating the despacho status.", Details = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDespacho(int id)
    {
        try
        {
            var result = await _despachoService.DeleteDespachoAsync(id);
            if (!result)
            {
                return NotFound(new { Message = $"Despacho with ID {id} not found." });
            }
            return NoContent(); // 204 No Content
        }
        catch (Exception ex)
        {
            // Log the exception
            return StatusCode(500, new { Message = "An error occurred while deleting the despacho.", Details = ex.Message });
        }
    }
}