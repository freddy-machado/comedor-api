using Comedor.Core.Dtos.Auth;
using Comedor.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization; 
using System.Collections.Generic; 
using System; // For InvalidOperationException

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

    [HttpPost("register")] // This is now the single creation method
    public async Task<IActionResult> Register(RegisterDto registerDto)
    {
        var result = await _authService.RegisterAsync(registerDto);
        if (!string.IsNullOrEmpty(result.Message))
        {
            return BadRequest(new { result.Message });
        }
        // For registration, we might return 201 Created, but Ok is fine for now
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto loginDto)
    {
        var result = await _authService.LoginAsync(loginDto);
        if (!result.IsAuthenticated.GetValueOrDefault())
        {
            return Unauthorized(new { result.Message });
        }
        return Ok(result);
    }

    // CRUD Endpoints for Users
    [HttpGet("users")]
    [Authorize] // Protect this endpoint
    public async Task<ActionResult<IEnumerable<UserListDto>>> GetAllUsers()
    {
        var users = await _authService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("users/{id}")]
    [Authorize] // Protect this endpoint
    public async Task<ActionResult<UserDto>> GetUserById(string id)
    {
        var user = await _authService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound(new { Message = $"User with ID {id} not found." });
        }
        return Ok(user);
    }

    [HttpGet("users/byusername/{userName}")]
    [Authorize] // Protect this endpoint
    public async Task<ActionResult<UserDto>> GetUserByUserName(string userName)
    {
        var user = await _authService.GetUserByUserNameAsync(userName);
        if (user == null)
        {
            return NotFound(new { Message = $"User with username {userName} not found." });
        }
        return Ok(user);
    }

    // Removed: [HttpPost("users")] CreateUser endpoint

    [HttpPut("users")]
    [Authorize] // Protect this endpoint
    public async Task<ActionResult<UserDto>> UpdateUser([FromBody] UpdateUserDto updateDto)
    {
        try
        {
            var result = await _authService.UpdateUserAsync(updateDto);
            if (result == null)
            {
                return NotFound(new { Message = $"User with ID {updateDto.Id} not found." });
            }
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPatch("users/{id}/status")]
    [Authorize] // Protect this endpoint
    public async Task<IActionResult> SetUserActiveStatus(string id, [FromQuery] bool isActive)
    {
        var result = await _authService.SetUserActiveStatusAsync(id, isActive);
        if (!result)
        {
            return NotFound(new { Message = $"User with ID {id} not found." });
        }
        return NoContent(); // 204 No Content
    }

    // New temporary endpoint to force normalize UserName
    [HttpPut("users/force-normalize-username/{userId}")]
    [Authorize] // Protect this endpoint
    public async Task<IActionResult> ForceNormalizeUserName(string userId)
    {
        var result = await _authService.ForceNormalizeUserNameAsync(userId);
        if (!result)
        {
            return BadRequest(new { Message = $"Failed to force normalize UserName for user ID '{userId}'. User might not exist or an error occurred." });
        }
        return Ok(new { Message = $"UserName and NormalizedUserName for user ID '{userId}' successfully re-normalized. Try logging in with the correct UserName." });
    }
}