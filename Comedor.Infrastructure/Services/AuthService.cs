using AutoMapper;
using Comedor.Core.Dtos.Auth;
using Comedor.Core.Entities;
using Comedor.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Comedor.Infrastructure.Data;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt; // <- Asegúrate del namespace correcto para ApplicationDbContext

namespace Comedor.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;
    // campo adicional
    private readonly ComedorDbContext _dbContext;

    public AuthService(UserManager<ApplicationUser> userManager, 
        RoleManager<IdentityRole> roleManager, 
        IConfiguration configuration, 
        IMapper mapper, 
        ILogger<AuthService> logger, 
        ComedorDbContext dbContext)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _mapper = mapper;
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<UserDto> LoginAsync(LoginDto loginDto)
    {
        _logger.LogInformation($"Attempting login for user: {loginDto.UserName}");

        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == loginDto.UserName);
        if (user == null)
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

        // Construir DTO básico y rellenar campos críticos explícitamente
        var userDto = _mapper.Map<UserDto>(user) ?? new UserDto();
        userDto.Id = user.Id;
        userDto.UserName = user.UserName ?? string.Empty;
        userDto.Email = user.Email ?? string.Empty;
        // Campo adicional: nombre de usuario de la aplicación (se puede ajustar según tu convención)
        userDto.UserNameApplication = user.NormalizedUserName ?? user.UserName ?? string.Empty;

        userDto.Token = await GenerateJwtToken(user);
        userDto.IsAuthenticated = true;

        // Obtener roles y claims directamente del UserManager (evitamos una segunda búsqueda por id)
        var roles = (await _userManager.GetRolesAsync(user)).ToList();
        userDto.Roles = roles;

        var claims = await _userManager.GetClaimsAsync(user);
        userDto.Claims = claims.Select(c => new ClaimDto { ClaimType = c.Type, ClaimValue = c.Value }).ToList();

        // Generar y almacenar refresh token al hacer login (rotación inicial)
        try
        {
            var (refreshToken, expiry) = await GenerateAndStoreRefreshTokenAsync(user.Id, TimeSpan.FromDays(7));
            userDto.RefreshToken = refreshToken;
            userDto.RefreshTokenExpiration = expiry;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate/store refresh token during login for user {UserId}", user.Id);
        }

        // poblar menús/permisos del usuario
        try
        {
            userDto.Menus = await BuildMenusForUserAsync(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build menus for user {UserId}", user.Id);
            userDto.Menus = new List<MenuDto>();
        }

        _logger.LogInformation($"Login successful for user: {loginDto.UserName}");
        return userDto;
    }

    // New CRUD methods
    public async Task<IEnumerable<UserListDto>> GetAllUsersAsync()
    {
        var users = _userManager.Users.ToList(); // Get all users
        return _mapper.Map<IEnumerable<UserListDto>>(users);
    }

    // Cambiado: ahora devuelve UpdateUserDto (coincide con la interfaz)
    public async Task<UpdateUserDto?> GetUserByIdAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return null;

        // Preferimos rellenar explícitamente los campos críticos para evitar dependencias del mapeo
        var dto = _mapper.Map<UpdateUserDto>(user) ?? new UpdateUserDto();

        dto.Id = user.Id;
        dto.UserName = user.UserName ?? string.Empty;
        dto.Email = user.Email ?? string.Empty;
        dto.PhoneNumber = user.PhoneNumber;
        dto.NormalizedUserName = user.NormalizedUserName ?? string.Empty;

        return dto;
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
        user.NormalizedUserName = updateDto.NormalizedUserName;

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

        // Hashear el token antes de guardar (no almacenamos el token en texto plano)
        var tokenHashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        var tokenHashBase64 = Convert.ToBase64String(tokenHashBytes);

        // store token hash and expiry as authentication tokens
        await _userManager.SetAuthenticationTokenAsync(user, "App", "RefreshToken", tokenHashBase64);
        await _userManager.SetAuthenticationTokenAsync(user, "App", "RefreshTokenExpiry", expiry.ToString("o"));

        return (token, expiry); // devolvemos el token en claro al cliente (solo aquí)
    }

    public async Task<bool> ValidateRefreshTokenAsync(string userId, string refreshToken)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        var storedTokenBase64 = await _userManager.GetAuthenticationTokenAsync(user, "App", "RefreshToken");
        var storedExpiry = await _userManager.GetAuthenticationTokenAsync(user, "App", "RefreshTokenExpiry");

        if (string.IsNullOrEmpty(storedTokenBase64)) return false;

        byte[] storedHashBytes;
        try
        {
            storedHashBytes = Convert.FromBase64String(storedTokenBase64);
        }
        catch
        {
            return false;
        }

        var providedHash = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));

        // Comparación en tiempo fijo
        if (!CryptographicOperations.FixedTimeEquals(providedHash, storedHashBytes)) return false;

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

    // Implementación recomendada (usa métodos ya existentes)
    public async Task<(string? Token, string? RefreshToken, DateTime? Expiry)> RefreshTokenAsync(string userId, string refreshToken, bool rotate = true)
    {
        // validar refresh token almacenado (hash)
        if (!await ValidateRefreshTokenAsync(userId, refreshToken))
            return (null, null, null);

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return (null, null, null);

        // generar nuevo JWT
        var token = await GenerateJwtToken(user);

        // opcional: rotar refresh token
        if (rotate)
        {
            var (newRefresh, expiry) = await GenerateAndStoreRefreshTokenAsync(userId, TimeSpan.FromDays(7));
            return (token, newRefresh, expiry);
        }

        return (token, null, null);
    }

    public async Task<(bool Succeeded, string[] Errors, UserDto? User)> CreateUserWithRolesAsync(RegisterDto dto)
    {
        if (dto == null) return (false, new[] { "Payload vacío." }, null);

        // Normalizar/limpiar roles y validar existencia
        var rolesToAssign = dto.Roles?.Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => r.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList() ?? new List<string>();
        var missing = new List<string>();
        foreach (var role in rolesToAssign)
        {
            if (!await _roleManager.RoleExistsAsync(role)) missing.Add(role);
        }
        if (missing.Any()) return (false, missing.Select(m => $"Role does not exist: {m}").ToArray(), null);

        using var tx = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var user = new ApplicationUser
            {
                UserName = dto.UserName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                IsActive = true,
                EmailConfirmed = false
            };

            // Normalizar en servidor (usando UserManager)
            user.NormalizedUserName = _userManager.NormalizeName(user.UserName);
            user.NormalizedEmail = _userManager.NormalizeEmail(user.Email);

            var createResult = await _userManager.CreateAsync(user, dto.Password);
            if (!createResult.Succeeded)
            {
                await tx.RollbackAsync();
                return (false, createResult.Errors.Select(e => e.Description).ToArray(), null);
            }

            if (rolesToAssign.Any())
            {
                var addRolesResult = await _userManager.AddToRolesAsync(user, rolesToAssign);
                if (!addRolesResult.Succeeded)
                {
                    // intentar borrar usuario si falla la asignación y hacer rollback
                    await _userManager.DeleteAsync(user);
                    await tx.RollbackAsync();
                    return (false, addRolesResult.Errors.Select(e => e.Description).ToArray(), null);
                }
            }

            await tx.CommitAsync();

            var userDto = _mapper.Map<UserDto>(user);
            userDto.Roles = (await _userManager.GetRolesAsync(user)).ToList();
            return (true, Array.Empty<string>(), userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creando usuario con roles");
            await tx.RollbackAsync();
            return (false, new[] { "Internal error creating user." }, null);
        }
    }

    public async Task<(bool Succeeded, string[] Errors, UserDto? User)> UpdateUserWithRolesAsync(UpdateUserDto dto)
    {
        if (dto == null) return (false, new[] { "Payload vacío." }, null);

        var user = await _userManager.FindByIdAsync(dto.Id);
        if (user == null) return (false, new[] { "User not found." }, null);

        var desiredRoles = dto.Roles?.Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => r.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList() ?? new List<string>();
        var missing = new List<string>();
        foreach (var role in desiredRoles)
        {
            if (!await _roleManager.RoleExistsAsync(role)) missing.Add(role);
        }
        if (missing.Any()) return (false, missing.Select(m => $"Role does not exist: {m}").ToArray(), null);

        using var tx = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            // Actualizar campos básicos
            user.UserName = dto.UserName;
            user.Email = dto.Email;
            user.PhoneNumber = dto.PhoneNumber;
            user.NormalizedUserName = _userManager.NormalizeName(dto.UserName); // normalizar en servidor

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                await tx.RollbackAsync();
                return (false, updateResult.Errors.Select(e => e.Description).ToArray(), null);
            }

            // Si se envió contraseña, realizar reset (token generado por servidor)
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                // Generar un token seguro para resetear la contraseña del usuario.
                // Se usa GeneratePasswordResetTokenAsync porque ResetPasswordAsync requiere un token válido,
                // y este token permite cambiar la contraseña sin conocer la anterior (flujo administrado por servidor).
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

                // Intentar aplicar el nuevo password usando el token generado.
                // ResetPasswordAsync valida el token y actualiza el hash de la contraseña en la base de datos.
                var resetResult = await _userManager.ResetPasswordAsync(user, resetToken, dto.Password);

                // Si la operación de reset falla, hacemos rollback de la transacción para evitar cambios parciales
                // (por ejemplo, que el email/username hayan sido actualizados pero la contraseña no).
                // Devolvemos los errores de Identity para que el controlador/cliente los presenten.
                if (!resetResult.Succeeded)
                {
                    await tx.RollbackAsync();
                    return (false, resetResult.Errors.Select(e => e.Description).ToArray(), null);
                }
            }

            var currentRoles = (await _userManager.GetRolesAsync(user)).ToList();
            var toAdd = desiredRoles.Except(currentRoles, StringComparer.OrdinalIgnoreCase).ToList();
            var toRemove = currentRoles.Except(desiredRoles, StringComparer.OrdinalIgnoreCase).ToList();

            if (toRemove.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, toRemove);
                if (!removeResult.Succeeded)
                {
                    await tx.RollbackAsync();
                    return (false, removeResult.Errors.Select(e => e.Description).ToArray(), null);
                }
            }

            if (toAdd.Any())
            {
                var addResult = await _userManager.AddToRolesAsync(user, toAdd);
                if (!addResult.Succeeded)
                {
                    await tx.RollbackAsync();
                    return (false, addResult.Errors.Select(e => e.Description).ToArray(), null);
                }
            }

            await tx.CommitAsync();

            var userDto = _mapper.Map<UserDto>(user);
            userDto.Roles = (await _userManager.GetRolesAsync(user)).ToList();
            return (true, Array.Empty<string>(), userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error actualizando usuario con roles");
            await tx.RollbackAsync();
            return (false, new[] { "Internal error updating user." }, null);
        }
    }


    private async Task<List<MenuDto>> BuildMenusForUserAsync(ApplicationUser user)
    {
        var roleNames = await _userManager.GetRolesAsync(user);

        var roleIds = await _roleManager.Roles
            .Where(r => roleNames.Contains(r.Name))
            .Select(r => r.Id)
            .ToListAsync();

        var rows = await _dbContext.RoleMenuActions
            .AsNoTracking()
            .Where(rma => roleIds.Contains(rma.RoleId))
            .Select(rma => new
            {
                rma.MenuId,
                MenuKey = rma.Menu.Key,
                MenuTitle = rma.Menu.Title,
                MenuHref = rma.Menu.Href,
                ParentId = rma.Menu.ParentId,
                ActionId = rma.ActionId,
                ActionKey = rma.Action.Key,
                ActionTitle = rma.Action.Title
            })
            .ToListAsync();

        var menusFlat = rows
            .GroupBy(r => new { r.MenuId, r.MenuKey, r.MenuTitle, r.MenuHref, r.ParentId })
            .Select(g => new MenuDto
            {
                Id = g.Key.MenuId,
                Key = g.Key.MenuKey,
                Title = g.Key.MenuTitle,
                Href = g.Key.MenuHref,
                AllowedActions = g
                    .Select(x => new ActionDto { Id = x.ActionId, Key = x.ActionKey, Title = x.ActionTitle })
                    .GroupBy(a => a.Id)
                    .Select(ga => ga.First())
                    .OrderBy(a => a.Id)
                    .ToList() // ahora devuelve List<ActionDto>, que coincide con MenuDto.AllowedActions
            })
            .ToList();

        var parentMap = rows
            .GroupBy(r => r.MenuId)
            .ToDictionary(g => g.Key, g => g.First().ParentId as int?);

        var dict = menusFlat.ToDictionary(m => m.Id);
        var rootMenus = new List<MenuDto>();

        foreach (var menu in menusFlat)
        {
            if (parentMap.TryGetValue(menu.Id, out var parentId) && parentId.HasValue && dict.TryGetValue(parentId.Value, out var parent))
            {
                parent.Children.Add(menu);
            }
            else
            {
                rootMenus.Add(menu);
            }
        }

        return rootMenus.OrderBy(m => m.Id).ToList();
    }
}