namespace AuthService_SB.Application.DTOs;

/// <summary>
/// Perfil extendido del usuario.
/// </summary>
public class UserProfileDto
{
    /// <summary>
    /// Identificador del perfil.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Identificador del usuario propietario del perfil.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// URL de imagen de perfil.
    /// </summary>
    public string ProfilePicture { get; set; } = string.Empty;

    /// <summary>
    /// Número telefónico del usuario.
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Documento personal de identificación.
    /// </summary>
    public string Dpi { get; set; } = string.Empty;

    /// <summary>
    /// Dirección del usuario.
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Nombre del trabajo o puesto laboral.
    /// </summary>
    public string JobName { get; set; } = string.Empty;

    /// <summary>
    /// Ingreso mensual reportado.
    /// </summary>
    public decimal MonthlyIncome { get; set; }
}
