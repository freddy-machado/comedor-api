using Comedor.Core.Dtos.Auth;
using Comedor.Core.Entities;
using Comedor.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Security.Claims;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Comedor.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IAuthService authService, UserManager<ApplicationUser> userManager, IMapper mapper, ILogger<AccountController> logger)
    {
        _authService = authService;
        _userManager = userManager;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _authService.LoginAsync(dto);
        if (user == null || !user.IsAuthenticated.GetValueOrDefault())
            return Unauthorized(user);

        return Ok(user);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Validaciones adicionales de seguridad/negocio
        if (await _userManager.FindByEmailAsync(dto.Email) is not null)
            return Conflict(new { Message = "Email ya registrado." });

        if (await _userManager.FindByNameAsync(dto.UserName) is not null)
            return Conflict(new { Message = "Nombre de usuario no disponible." });

        var (succeeded, errors, userDto) = await _authService.CreateUserWithRolesAsync(dto);
        if (!succeeded)
            return BadRequest(new { Message = "No se pudo crear el usuario.", Errors = errors });

        return CreatedAtAction(nameof(GetUser), new { id = userDto!.Id }, userDto);
    }

    // Nuevo: activar usuario (Admin only)
    [HttpPost("users/{id}/activate"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> ActivateUser(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return BadRequest();

        var ok = await _authService.SetUserActiveStatusAsync(id, true);
        if (!ok) return NotFound(new { Message = "Usuario no encontrado o no se pudo activar." });

        return Ok(new { Message = "Usuario activado." });
    }

    // Nuevo: desactivar usuario (Admin only)
    [HttpPost("users/{id}/deactivate"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeactivateUser(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return BadRequest();

        var ok = await _authService.SetUserActiveStatusAsync(id, false);
        if (!ok) return NotFound(new { Message = "Usuario no encontrado o no se pudo desactivar." });

        return Ok(new { Message = "Usuario desactivado." });
    }

    [HttpGet("users"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _authService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("users/{id}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUser(string id)
    {
        var user = await _authService.GetUserByIdAsync(id);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpPut("users/{id}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDto dto)
    {
        if (id != dto.Id) return BadRequest();

        // Llamamos al servicio que actualiza el usuario y sincroniza sus roles en una sola operación atómica.
        // Devuelve una tupla: (Succeeded, Errors[], UserDto?)
        // - Succeeded: true si toda la operación (update + roles) se realizó correctamente.
        // - Errors: mensajes detallados cuando algo falla (validación, roles inexistentes, errores de Identity).
        // - UserDto: representación del usuario actualizado (puede ser null si no se logró actualizar).
        // Esto nos permite procesar errores y devolver códigos HTTP apropiados desde el controlador.
        var (succeeded, errors, userDto) = await _authService.UpdateUserWithRolesAsync(dto);
        if (!succeeded)
        {
            if (errors.Contains("User not found")) return NotFound();
            return BadRequest(new { Errors = errors });
        }

        return Ok(userDto);
    }

    // Roles
    [HttpPost("roles"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateRole([FromBody] RoleDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Role))
            return BadRequest("Role is required.");

        var ok = await _authService.CreateRoleAsync(dto.Role);
        if (!ok) return BadRequest();
        return Ok();
    }

    [HttpGet("roles"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _authService.GetAllRolesAsync();
        return Ok(roles);
    }

    [HttpPost("users/{id}/roles"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddRoleToUser(string id, [FromBody] RoleDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Role))
            return BadRequest("Role is required.");

        var ok = await _authService.AssignRoleToUserAsync(id, dto.Role);
        if (!ok) return BadRequest();
        return Ok();
    }

    [HttpDelete("users/{id}/roles/{role}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> RemoveRoleFromUser(string id, string role)
    {
        var ok = await _authService.RemoveRoleFromUserAsync(id, role);
        if (!ok) return BadRequest();
        return Ok();
    }

    // New: obtener roles de un usuario
    [HttpGet("users/{id}/roles"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUserRoles(string id)
    {
        var roles = await _authService.GetUserRolesAsync(id);
        return Ok(roles);
    }

    // Claims
    [HttpPost("users/{id}/claims"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddClaimToUser(string id, [FromBody] ClaimDto claim)
    {
        var ok = await _authService.AddClaimToUserAsync(id, claim);
        if (!ok) return BadRequest();
        return Ok();
    }

    [HttpDelete("users/{id}/claims"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> RemoveClaimFromUser(string id, [FromBody] ClaimDto claim)
    {
        var ok = await _authService.RemoveClaimFromUserAsync(id, claim);
        if (!ok) return BadRequest();
        return Ok();
    }

    [HttpGet("users/{id}/claims"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUserClaims(string id)
    {
        var claims = await _authService.GetUserClaimsAsync(id);
        return Ok(claims);
    }

    // Role claims: añadir y listar claims asociados a un rol
    [HttpPost("roles/{role}/claims"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddClaimToRole(string role, [FromBody] ClaimDto claim)
    {
        var ok = await _authService.AddClaimToRoleAsync(role, claim);
        if (!ok) return BadRequest();
        return Ok();
    }

    [HttpGet("roles/{role}/claims"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetRoleClaims(string role)
    {
        var claims = await _authService.GetRoleClaimsAsync(role);
        return Ok(claims);
    }

    // Refresh token endpoints
    [HttpPost("users/{id}/refresh/generate"), Authorize]
    public async Task<IActionResult> GenerateRefreshToken(string id)
    {
        // Validación: sólo el propio usuario (por NameIdentifier) o Admin pueden generar
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (currentUserId != id && !User.IsInRole("Admin"))
            return Forbid();

        var (token, expiry) = await _authService.GenerateAndStoreRefreshTokenAsync(id, TimeSpan.FromDays(7));
        return Ok(new { RefreshToken = token, Expiry = expiry });
    }

    [HttpPost("refresh/validate")]
    public async Task<IActionResult> ValidateRefresh([FromBody] RefreshValidateDto dto)
    {
        var ok = await _authService.ValidateRefreshTokenAsync(dto.UserId, dto.RefreshToken);
        if (!ok) return Unauthorized();
        return Ok();
    }

    [HttpPost("users/{id}/refresh/revoke"), Authorize]
    public async Task<IActionResult> RevokeRefresh(string id)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (currentUserId != id && !User.IsInRole("Admin"))
            return Forbid();

        var ok = await _authService.RevokeRefreshTokenAsync(id);
        if (!ok) return BadRequest();
        return Ok();
    }

    [HttpPost("users/by-username/{userName}/roles"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddRoleToUserByUserName(string userName, [FromBody] RoleDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Role))
            return BadRequest("Role is required.");

        var user = await _authService.GetUserByUserNameAsync(userName);
        if (user == null) return NotFound();
        var ok = await _authService.AssignRoleToUserAsync(user.Id, dto.Role);
        if (!ok) return BadRequest();
        return Ok();
    }

    // Endpoint nuevo para intercambio refresh -> access token
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshValidateDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.UserId) || string.IsNullOrWhiteSpace(dto.RefreshToken))
            return BadRequest();

        var (token, refreshToken, expiry) = await _authService.RefreshTokenAsync(dto.UserId, dto.RefreshToken);
        if (string.IsNullOrEmpty(token)) return Unauthorized();

        return Ok(new { Token = token, RefreshToken = refreshToken, Expiry = expiry });
    }
}