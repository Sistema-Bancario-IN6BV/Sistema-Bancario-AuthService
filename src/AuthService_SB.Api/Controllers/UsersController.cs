using AuthService_SB.Application.DTOs;
using AuthService_SB.Application.Interfaces;
using AuthService_SB.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AuthService_SB.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class UsersController(IUserManagementService userManagementService, IAuthService authService) : ControllerBase
{
    private async Task<bool> CurrentUserIsAdmin()
    {
        var userId = User.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
        if (string.IsNullOrEmpty(userId)) return false;
        var roles = await userManagementService.GetUserRolesAsync(userId);
        return roles.Contains(RoleConstants.ADMIN_ROLE);
    }

    [HttpPost]
    [Authorize]
    [RequestSizeLimit(10 * 1024 * 1024)]
    [EnableRateLimiting("ApiPolicy")]
    public async Task<ActionResult<object>> CreateUser([FromForm] CreateUserByAdminDto createUserByAdminDto)
    {
        if (!await CurrentUserIsAdmin())
        {
            return StatusCode(403, new { success = false, message = "Solo administradores pueden crear usuarios" });
        }

        var user = await authService.CreateUserByAdminAsync(createUserByAdminDto);
        if (user == null)
        {
            return BadRequest(new
            {
                success = false,
                message = "Error al crear el usuario"
            });
        }

        return StatusCode(201, new
        {
            success = true,
            message = "Usuario creado exitosamente",
            data = user
        });
    }

    [HttpGet("{userId}")]
    [Authorize]
    [EnableRateLimiting("ApiPolicy")]
    public async Task<ActionResult<object>> GetUserById(string userId)
    {
        if (!await CurrentUserIsAdmin())
        {
            return StatusCode(403, new { success = false, message = "Forbidden" });
        }

        var user = await authService.GetUserByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new { success = false, message = "Usuario no encontrado" });
        }

        return Ok(new
        {
            success = true,
            message = "Usuario obtenido exitosamente",
            data = user
        });
    }

    [HttpPut("{userId}")]
    [Authorize]
    [RequestSizeLimit(10 * 1024 * 1024)]
    [EnableRateLimiting("ApiPolicy")]
    public async Task<ActionResult<object>> UpdateUser(string userId, [FromForm] UpdateUserByAdminDto updateUserByAdminDto)
    {
        if (!await CurrentUserIsAdmin())
        {
            return StatusCode(403, new { success = false, message = "Solo administradores pueden actualizar usuarios" });
        }

        var user = await authService.UpdateUserByAdminAsync(userId, updateUserByAdminDto);
        if (user == null)
        {
            return BadRequest(new
            {
                success = false,
                message = "Error al actualizar el usuario"
            });
        }

        return Ok(new
        {
            success = true,
            message = "Usuario actualizado exitosamente",
            data = user
        });
    }

    [HttpDelete("{userId}")]
    [Authorize]
    [EnableRateLimiting("ApiPolicy")]
    public async Task<ActionResult<object>> DeleteUser(string userId)
    {
        if (!await CurrentUserIsAdmin())
        {
            return StatusCode(403, new { success = false, message = "Solo administradores pueden eliminar usuarios" });
        }

        var result = await authService.DeleteUserAsync(userId);
        if (!result)
        {
            return BadRequest(new
            {
                success = false,
                message = "Error al eliminar el usuario"
            });
        }

        return Ok(new
        {
            success = true,
            message = "Usuario eliminado exitosamente"
        });
    }

    [HttpPut("{userId}/role")]
    [Authorize]
    [EnableRateLimiting("ApiPolicy")]
    public async Task<ActionResult<UserResponseDto>> UpdateUserRole(string userId, [FromBody] UpdateUserRoleDto dto)
    {
        if (!await CurrentUserIsAdmin())
        {
            return StatusCode(403, new { success = false, message = "Forbidden" });
        }

        var result = await userManagementService.UpdateUserRoleAsync(userId, dto.RoleName);
        return Ok(result);
    }

    [HttpGet("{userId}/roles")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<string>>> GetUserRoles(string userId)
    {
        var roles = await userManagementService.GetUserRolesAsync(userId);
        return Ok(roles);
    }

    [HttpGet("by-role/{roleName}")]
    [Authorize]
    [EnableRateLimiting("ApiPolicy")]
    public async Task<ActionResult<IReadOnlyList<UserResponseDto>>> GetUsersByRole(string roleName)
    {
        if (!await CurrentUserIsAdmin())
        {
            return StatusCode(403, new { success = false, message = "Forbidden" });
        }
        
        var users = await userManagementService.GetUsersByRoleAsync(roleName);
        return Ok(users);
    }
}
