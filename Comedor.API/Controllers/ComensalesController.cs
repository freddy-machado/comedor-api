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
[Authorize]
public class ComensalesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AutoMapper.IMapper _mapper;
    private readonly IComensalService _comensalService;

    public ComensalesController(IUnitOfWork unitOfWork, AutoMapper.IMapper mapper, IComensalService comensalService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
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
        var comensalDto = await _comensalService.GetComensalCreateDtoByIdAsync(id);
        if (comensalDto == null)
        {
            return NotFound();
        }
        return Ok(comensalDto);
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

    [HttpPost("upsert")]
    public async Task<IActionResult> UpsertComensal([FromBody] ComensalCreateDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Identificacion))
            return BadRequest("El objeto Comensal es nulo o la identificación es obligatoria.");

        try
        {
            var result = await _comensalService.UpsertComensalAsync(dto);
            if (result == null)
                return NotFound();

            // Si el resultado tiene un Id, asumimos que fue creado o actualizado correctamente
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}
