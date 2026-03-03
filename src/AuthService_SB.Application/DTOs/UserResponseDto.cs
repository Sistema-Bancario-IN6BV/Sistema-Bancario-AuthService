namespace AuthService_SB.Application.DTOs;

/// <summary>
/// Datos públicos y administrativos del usuario.
/// </summary>
public class UserResponseDto
{
    /// <summary>
    /// Identificador único del usuario.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Nombre del usuario.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Apellido del usuario.
    /// </summary>
    public string Surname { get; set; } = string.Empty;

    /// <summary>
    /// Nombre de usuario.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Correo electrónico.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// URL de imagen de perfil.
    /// </summary>
    public string ProfilePicture { get; set; } = string.Empty;

    /// <summary>
    /// Teléfono del usuario.
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
    /// Ingreso mensual del usuario.
    /// </summary>
    public decimal MonthlyIncome { get; set; }

    /// <summary>
    /// Rol asignado al usuario.
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Estado de activación del usuario.
    /// </summary>
    public bool Status { get; set; }

    /// <summary>
    /// Indica si el correo electrónico fue verificado.
    /// </summary>
    public bool IsEmailVerified { get; set; }

    /// <summary>
    /// Fecha de creación del usuario en UTC.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Fecha de última actualización del usuario en UTC.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}