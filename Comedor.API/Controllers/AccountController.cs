using Comedor.Core.Dtos.Auth;
using Comedor.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Comedor.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IAuthService _authService;

    public AccountController(IAuthService authService)
    {
        _authService = authService;
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
        var created = await _authService.RegisterAsync(dto);
        if (!string.IsNullOrEmpty(created.Message))
            return BadRequest(created);
        return Ok(created);
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
        var updated = await _authService.UpdateUserAsync(dto);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    // Roles
    [HttpPost("roles"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateRole([FromBody] string role)
    {
        var ok = await _authService.CreateRoleAsync(role);
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
    public async Task<IActionResult> AddRoleToUser(string id, [FromBody] string role)
    {
        var ok = await _authService.AssignRoleToUserAsync(id, role);
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

    // Refresh token endpoints
    [HttpPost("users/{id}/refresh/generate"), Authorize]
    public async Task<IActionResult> GenerateRefreshToken(string id)
    {
        // only allow user itself or admin to generate token
        if (User.Identity?.Name != id && !User.IsInRole("Admin")) { /* you may adapt check */ }

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
        var ok = await _authService.RevokeRefreshTokenAsync(id);
        if (!ok) return BadRequest();
        return Ok();
    }
}