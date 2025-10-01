using System;
using Comedor.Core.Dtos;
using Comedor.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Comedor.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // Protect this controller
public class ComensalesController : ControllerBase
{
    private readonly IComensalService _comensalService;

    public ComensalesController(IComensalService comensalService)
    {
        _comensalService = comensalService;
    }

    [HttpGet]
    public async Task<IActionResult> GetComensales([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _comensalService.GetComensalesAsync(search, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id}", Name = "GetComensalById")]
    public async Task<IActionResult> GetComensalById(int id)
    {
        var comensalDto = await _comensalService.GetComensalByIdAsync(id);
        if (comensalDto == null)
        {
            return NotFound();
        }
        return Ok(comensalDto);
    }

    [HttpPost]
    public async Task<IActionResult> CreateComensal([FromBody] ComensalCreateDto createDto)
    {
        if (createDto == null)
        {
            return BadRequest("Comensal object is null");
        }

        try
        {
            var createdComensal = await _comensalService.CreateComensalAsync(createDto);
            return CreatedAtRoute("GetComensalById", new { id = createdComensal.Id }, createdComensal);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateComensal(int id, [FromBody] ComensalCreateDto updateDto)
    {
        if (updateDto == null)
        {
            return BadRequest("Invalid comensal data");
        }

        try
        {
            var result = await _comensalService.UpdateComensalAsync(id, updateDto);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("inactivate/{id}")]
    public async Task<IActionResult> InactivateComensal(int id)
    {
        var result = await _comensalService.InactivateComensalAsync(id);
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpGet("all-active")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _comensalService.GetAllActiveComensalesAsync();
        return Ok(result);
    }
}
