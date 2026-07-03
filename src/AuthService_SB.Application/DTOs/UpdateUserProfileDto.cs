//UpdateUserProfileDto.cs
using AuthService_SB.Application.Interfaces;

namespace AuthService_SB.Application.DTOs;

/// <summary>
/// Datos para actualizar perfil del usuario autenticado.
/// </summary>
public class UpdateUserProfileDto
{
    /// <summary>
    /// Nombre del usuario.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Apellido del usuario.
    /// </summary>
    public string Surname { get; set; } = string.Empty;

    /// <summary>
    /// Correo electrónico del usuario.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Nombre de usuario.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Archivo de foto de perfil.
    /// </summary>
    public IFileData? ProfilePicture { get; set; }

    /// <summary>
    /// Teléfono del usuario.
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Dirección del usuario.
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Nombre del trabajo o puesto laboral.
    /// </summary>
    public string JobName { get; set; } = string.Empty;

    /// <summary>
    /// Ingreso mensual del usuario.
    /// </summary>
    public decimal MonthlyIncome { get; set; }
}
