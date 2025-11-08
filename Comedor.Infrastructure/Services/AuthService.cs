using AutoMapper;
using Comedor.Core.Dtos.Auth;
using Comedor.Core.Entities;
using Comedor.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Comedor.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;

    public AuthService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, IMapper mapper, ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<UserDto> LoginAsync(LoginDto loginDto)
    {
        _logger.LogInformation($"Attempting login for user: {loginDto.UserName}");

        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == loginDto.UserName);
        if(user == null)
        {
            _logger.LogWarning($"User '{loginDto.UserName}' not found by exact UserName match. Trying NormalizedUserName match...");
            user = await _userManager.Users.FirstOrDefaultAsync(u => u.NormalizedUserName == loginDto.UserName.ToUpper());
            if (user == null)
            {
                _logger.LogWarning($"User '{loginDto.UserName}' not found by NormalizedUserName match either.");
                return new UserDto { Message = "Invalid user name or password." };
            }
            _logger.LogInformation($"User '{loginDto.UserName}' found by NormalizedUserName match.");
        }

        _logger.LogInformation($"User '{loginDto.UserName}' found. IsActive: {user.IsActive}");

        if (!user.IsActive)
        {
            _logger.LogWarning($"User '{loginDto.UserName}' is inactive.");
            return new UserDto { Message = "Invalid user name or password." };
        }

        var isPasswordCorrect = await _userManager.CheckPasswordAsync(user, loginDto.Password);
        if (!isPasswordCorrect)
        {
            _logger.LogWarning($"Password incorrect for user '{loginDto.UserName}'.");
            return new UserDto { Message = "Invalid user name or password." };
        }

        var userDto = _mapper.Map<UserDto>(user);
        userDto.Token = await GenerateJwtToken(user);
        userDto.IsAuthenticated = true;
        _logger.LogInformation($"Login successful for user: {loginDto.UserName}");
        return userDto;
    }

    public async Task<UserDto> RegisterAsync(RegisterDto registerDto)
    {
        var user = new ApplicationUser
        {
            UserName = registerDto.UserName,
            Email = registerDto.Email,
            PhoneNumber = registerDto.PhoneNumber, // New field
            IsActive = registerDto.IsActive // New field
        };
        var result = await _userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded)
        {
            var message = string.Join("\n", result.Errors.Select(e => e.Description));
            return new UserDto { Message = message };
        }

        var userDto = _mapper.Map<UserDto>(user);
        return userDto;
    }

    // New CRUD methods
    public async Task<IEnumerable<UserListDto>> GetAllUsersAsync()
    {
        var users = _userManager.Users.ToList(); // Get all users
        return _mapper.Map<IEnumerable<UserListDto>>(users);
    }

    public async Task<UserDto?> GetUserByIdAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto?> GetUserByUserNameAsync(string userName)
    {
        var user = await _userManager.FindByNameAsync(userName);
        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto?> UpdateUserAsync(UpdateUserDto updateDto)
    {
        var user = await _userManager.FindByIdAsync(updateDto.Id);
        if (user == null)
        {
            return null; // User not found
        }

        user.UserName = updateDto.UserName;
        user.Email = updateDto.Email;
        user.PhoneNumber = updateDto.PhoneNumber; // New field
        user.IsActive = updateDto.IsActive;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var message = string.Join("\n", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException(message);
        }

        return _mapper.Map<UserDto>(user);
    }

    public async Task<bool> SetUserActiveStatusAsync(string id, bool isActive)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return false; // User not found
        }

        user.IsActive = isActive;
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> UpdateUserNameAndNormalizedUserNameAsync(string currentUserName, string newUserName)
    {
        var user = await _userManager.FindByNameAsync(currentUserName);
        if (user == null)
        {
            _logger.LogWarning($"User '{currentUserName}' not found for UserName update.");
            return false;
        }

        var setUserNameResult = await _userManager.SetUserNameAsync(user, newUserName);
        if (!setUserNameResult.Succeeded)
        {
            _logger.LogError($"Failed to set UserName for user '{currentUserName}': {string.Join(", ", setUserNameResult.Errors.Select(e => e.Description))}");
            return false;
        }

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            _logger.LogError($"Failed to update user '{newUserName}' after setting UserName: {string.Join(", ", updateResult.Errors.Select(e => e.Description))}");
            return false;
        }

        _logger.LogInformation($"UserName for user '{currentUserName}' successfully updated to '{newUserName}'.");
        return true;
    }

    public async Task<bool> ForceNormalizeUserNameAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning($"User with ID '{userId}' not found for force normalization.");
            return false;
        }

        user.NormalizedUserName = _userManager.NormalizeName(user.UserName);
        user.NormalizedEmail = _userManager.NormalizeEmail(user.Email);

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            _logger.LogError($"Failed to force normalize UserName for user '{user.UserName}': {string.Join(", ", updateResult.Errors.Select(e => e.Description))}");
            return false;
        }

        _logger.LogInformation($"UserName and NormalizedUserName for user '{user.UserName}' successfully re-normalized.");
        return true;
    }

    // Roles
    public async Task<bool> CreateRoleAsync(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName)) return false;
        if (await _roleManager.RoleExistsAsync(roleName)) return false;
        var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
        return result.Succeeded;
    }

    public async Task<IEnumerable<string>> GetAllRolesAsync()
    {
        return await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
    }

    public async Task<bool> AssignRoleToUserAsync(string userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || !await _roleManager.RoleExistsAsync(roleName)) return false;
        var result = await _userManager.AddToRoleAsync(user, roleName);
        return result.Succeeded;
    }

    public async Task<bool> RemoveRoleFromUserAsync(string userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;
        var result = await _userManager.RemoveFromRoleAsync(user, roleName);
        return result.Succeeded;
    }

    public async Task<IEnumerable<string>> GetUserRolesAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return Enumerable.Empty<string>();
        return await _userManager.GetRolesAsync(user);
    }

    // User claims
    public async Task<bool> AddClaimToUserAsync(string userId, ClaimDto claim)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;
        var result = await _userManager.AddClaimAsync(user, new Claim(claim.ClaimType, claim.ClaimValue));
        return result.Succeeded;
    }

    public async Task<bool> RemoveClaimFromUserAsync(string userId, ClaimDto claim)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;
        var result = await _userManager.RemoveClaimAsync(user, new Claim(claim.ClaimType, claim.ClaimValue));
        return result.Succeeded;
    }

    public async Task<IEnumerable<ClaimDto>> GetUserClaimsAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return Enumerable.Empty<ClaimDto>();
        var claims = await _userManager.GetClaimsAsync(user);
        return claims.Select(c => new ClaimDto { ClaimType = c.Type, ClaimValue = c.Value });
    }

    // Role claims
    public async Task<bool> AddClaimToRoleAsync(string roleName, ClaimDto claim)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        if (role == null) return false;
        var result = await _roleManager.AddClaimAsync(role, new Claim(claim.ClaimType, claim.ClaimValue));
        return result.Succeeded;
    }

    public async Task<IEnumerable<ClaimDto>> GetRoleClaimsAsync(string roleName)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        if (role == null) return Enumerable.Empty<ClaimDto>();
        var claims = await _roleManager.GetClaimsAsync(role);
        return claims.Select(c => new ClaimDto { ClaimType = c.Type, ClaimValue = c.Value });
    }

    // Refresh tokens stored in AspNetUserTokens
    private string GenerateSecureToken(int size = 64)
    {
        var bytes = new byte[size];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    public async Task<(string RefreshToken, DateTime Expiry)> GenerateAndStoreRefreshTokenAsync(string userId, TimeSpan validity)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) throw new InvalidOperationException("User not found");

        var token = GenerateSecureToken();
        var expiry = DateTime.UtcNow.Add(validity);

        // store token and expiry as authentication tokens
        await _userManager.SetAuthenticationTokenAsync(user, "App", "RefreshToken", token);
        await _userManager.SetAuthenticationTokenAsync(user, "App", "RefreshTokenExpiry", expiry.ToString("o"));

        return (token, expiry);
    }

    public async Task<bool> ValidateRefreshTokenAsync(string userId, string refreshToken)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        var storedToken = await _userManager.GetAuthenticationTokenAsync(user, "App", "RefreshToken");
        var storedExpiry = await _userManager.GetAuthenticationTokenAsync(user, "App", "RefreshTokenExpiry");

        if (string.IsNullOrEmpty(storedToken) || storedToken != refreshToken) return false;
        if (!DateTime.TryParse(storedExpiry, null, System.Globalization.DateTimeStyles.RoundtripKind, out var expiry)) return false;
        return DateTime.UtcNow <= expiry;
    }

    public async Task<bool> RevokeRefreshTokenAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        await _userManager.RemoveAuthenticationTokenAsync(user, "App", "RefreshToken");
        await _userManager.RemoveAuthenticationTokenAsync(user, "App", "RefreshTokenExpiry");
        return true;
    }

    private async Task<string> GenerateJwtToken(ApplicationUser user)
    {
        var jwtSettings = _configuration.GetSection("JWT");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id)
        };

        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles) 
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["DurationInMinutes"])),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}