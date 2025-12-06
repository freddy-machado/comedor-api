using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Comedor.Core.Dtos.Auth;

namespace Comedor.Core.Interfaces
{
    public interface IAuthService
    {
        Task<UserDto> LoginAsync(LoginDto loginDto);
        //Task<UserDto> RegisterAsync(RegisterDto registerDto);

        // CRUD methods for users
        Task<IEnumerable<UserListDto>> GetAllUsersAsync();
        Task<UpdateUserDto?> GetUserByIdAsync(string id);
        Task<UserDto?> GetUserByUserNameAsync(string userName);
        Task<UserDto?> UpdateUserAsync(UpdateUserDto updateDto);
        Task<bool> SetUserActiveStatusAsync(string id, bool isActive);
        Task<bool> UpdateUserNameAndNormalizedUserNameAsync(string currentUserName, string newUserName);
        Task<bool> ForceNormalizeUserNameAsync(string userId);

        // Roles & claims
        Task<bool> CreateRoleAsync(string roleName);
        Task<IEnumerable<string>> GetAllRolesAsync();
        Task<bool> AssignRoleToUserAsync(string userId, string roleName);
        Task<bool> RemoveRoleFromUserAsync(string userId, string roleName);
        Task<IEnumerable<string>> GetUserRolesAsync(string userId);

        Task<bool> AddClaimToUserAsync(string userId, ClaimDto claim);
        Task<bool> RemoveClaimFromUserAsync(string userId, ClaimDto claim);
        Task<IEnumerable<ClaimDto>> GetUserClaimsAsync(string userId);

        Task<bool> AddClaimToRoleAsync(string roleName, ClaimDto claim);
        Task<IEnumerable<ClaimDto>> GetRoleClaimsAsync(string roleName);

        // Refresh token management
        Task<(string RefreshToken, DateTime Expiry)> GenerateAndStoreRefreshTokenAsync(string userId, TimeSpan validity);
        Task<bool> ValidateRefreshTokenAsync(string userId, string refreshToken);
        Task<bool> RevokeRefreshTokenAsync(string userId);
        Task<(string? Token, string? RefreshToken, DateTime? Expiry)> RefreshTokenAsync(string userId, string refreshToken, bool rotate = true);

        // Nuevos métodos atómicos que crean/actualizan usuario y asignan roles en una transacción
        Task<(bool Succeeded, string[] Errors, UserDto? User)> CreateUserWithRolesAsync(RegisterDto dto);
        Task<(bool Succeeded, string[] Errors, UserDto? User)> UpdateUserWithRolesAsync(UpdateUserDto dto);
    }
}