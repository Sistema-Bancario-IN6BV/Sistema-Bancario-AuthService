using AuthService_SB.Application.DTOs;
using AuthService_SB.Application.Interfaces;
using AuthService_SB.Api.Models;
using AuthService_SB.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AuthService_SB.Api.Controllers;

/// <summary>
/// Endpoints de administración de usuarios y roles.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
// Aplicamos Authorize a nivel de clase con el rol ADMIN_ROLE para que 
// por defecto ningún endpoint sea accesible por alguien que no sea administrador.
[Authorize(Roles = RoleConstants.ADMIN_ROLE)] 
public class UsersController(IUserManagementService userManagementService, IAuthService authService) : ControllerBase
{
    /// <summary>
    /// Crea un usuario desde la consola administrativa.
    /// </summary>
    [HttpPost]
    [RequestSizeLimit(10 * 1024 * 1024)]
    [EnableRateLimiting("ApiPolicy")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<object>> CreateUser([FromForm] CreateUserByAdminDto createUserByAdminDto)
    {
        // Al usar [Authorize(Roles = ...)], .NET ya validó el token y el rol antes de entrar aquí.
        var user = await authService.CreateUserByAdminAsync(createUserByAdminDto);
        
        if (user == null)
        {
            return BadRequest(new { success = false, message = "Error al crear el usuario. Verifique si el correo o DPI ya existen." });
        }

        return StatusCode(201, new
        {
            success = true,
            message = "Usuario creado exitosamente",
            data = user
        });
    }

    /// <summary>
    /// Obtiene un usuario por su identificador.
    /// </summary>
    [HttpGet("{userId}")]
    [EnableRateLimiting("ApiPolicy")]
    public async Task<ActionResult<object>> GetUserById(string userId)
    {
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

    /// <summary>
    /// Actualiza un usuario como administrador.
    /// </summary>
    [HttpPut("{userId}")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<object>> UpdateUser(string userId, [FromForm] UpdateUserByAdminDto updateUserByAdminDto)
    {
        var user = await authService.UpdateUserByAdminAsync(userId, updateUserByAdminDto);
        if (user == null)
        {
            return BadRequest(new { success = false, message = "Error al actualizar el usuario" });
        }

        return Ok(new
        {
            success = true,
            message = "Usuario actualizado exitosamente",
            data = user
        });
    }

    /// <summary>
    /// Elimina un usuario por su identificador.
    /// </summary>
    [HttpDelete("{userId}")]
    public async Task<ActionResult<object>> DeleteUser(string userId)
    {
        var result = await authService.DeleteUserAsync(userId);
        if (!result)
        {
            return BadRequest(new { success = false, message = "Error al eliminar el usuario" });
        }

        return Ok(new { success = true, message = "Usuario eliminado exitosamente" });
    }

    /// <summary>
    /// Cambia el rol de un usuario.
    /// </summary>
    [HttpPut("{userId}/role")]
    public async Task<ActionResult<UserResponseDto>> UpdateUserRole(string userId, [FromBody] UpdateUserRoleDto dto)
    {
        var result = await userManagementService.UpdateUserRoleAsync(userId, dto.RoleName);
        return Ok(new { success = true, message = "Rol actualizado", data = result });
    }

    /// <summary>
    /// Obtiene los roles asignados a un usuario.
    /// </summary>
    [HttpGet("{userId}/roles")]
    // Permitimos que cualquier usuario autenticado vea sus propios roles o los de otros
    [Authorize] 
    public async Task<ActionResult<IReadOnlyList<string>>> GetUserRoles(string userId)
    {
        var roles = await userManagementService.GetUserRolesAsync(userId);
        return Ok(roles);
    }

    /// <summary>
    /// Lista usuarios filtrados por rol.
    /// </summary>
    [HttpGet("by-role/{roleName}")]
    public async Task<ActionResult<IReadOnlyList<UserResponseDto>>> GetUsersByRole(string roleName)
    {
        var users = await userManagementService.GetUsersByRoleAsync(roleName);
        return Ok(users);
    }
}