using AuthService_SB.Application.DTOs;

namespace AuthService_SB.Application.Interfaces;

public interface IUserManagementService
{
    Task<IReadOnlyList<UserResponseDto>> GetAllUsersAsync();
    Task<UserResponseDto> UpdateUserRoleAsync(string userId, string roleName);
    Task<IReadOnlyList<string>> GetUserRolesAsync(string userId);
    Task<IReadOnlyList<UserResponseDto>> GetUsersByRoleAsync(string roleName);
}