namespace AuthService_SB.Application.DTOs;

/// <summary>
/// Datos para actualizar perfil del usuario autenticado.
/// </summary>
public class UpdateUserProfileDto
{
    /// <summary>
    /// Ruta o URL de la foto de perfil.
    /// </summary>
    public string ProfilePicture { get; set; } = string.Empty;

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
