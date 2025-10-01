using Comedor.Core.Dtos.Auth;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Comedor.Core.Interfaces;

public interface IAuthService
{
    Task<UserDto> LoginAsync(LoginDto loginDto);
    Task<UserDto> RegisterAsync(RegisterDto registerDto);

    // CRUD methods for users
    Task<IEnumerable<UserListDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserByIdAsync(string id);
    Task<UserDto?> GetUserByUserNameAsync(string userName);
    Task<UserDto?> UpdateUserAsync(UpdateUserDto updateDto);
    Task<bool> SetUserActiveStatusAsync(string id, bool isActive);
    Task<bool> ForceNormalizeUserNameAsync(string userId); // New method
}