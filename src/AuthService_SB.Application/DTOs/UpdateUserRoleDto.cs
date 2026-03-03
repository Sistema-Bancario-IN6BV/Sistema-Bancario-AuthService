namespace AuthService_SB.Application.DTOs;

/// <summary>
/// Datos para actualizar el rol de un usuario.
/// </summary>
public class UpdateUserRoleDto
{
    /// <summary>
    /// Nombre del rol a asignar.
    /// </summary>
    public string RoleName { get; set; } = string.Empty;
}