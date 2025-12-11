using Comedor.Core.Dtos;
using Comedor.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Comedor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PermissionsController : ControllerBase
{
    private readonly PermissionService _perm;

    public PermissionsController(PermissionService perm) => _perm = perm;

    // Devuelve el árbol de menú filtrado por usuario (autorizado)
    // Permite solo al propio usuario o a Admins
    [HttpGet("user/{id}/menu"), Authorize]
    public async Task<IActionResult> GetMenuForUser(string id)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isAdmin = User.IsInRole("Admin");

        if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

        // Si no es admin y pide otro usuario, denegar
        if (!isAdmin && currentUserId != id) return Forbid();

        var items = await _perm.GetMenuForUserAsync(id);
        return Ok(items);
    }

    // Alternativa: devolver menú del usuario actual
    [HttpGet("me/menu"), Authorize]
    public async Task<IActionResult> GetMyMenu()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var items = await _perm.GetMenuForUserAsync(userId);
        return Ok(items);
    }
}