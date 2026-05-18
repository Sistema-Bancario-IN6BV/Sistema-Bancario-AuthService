namespace AuthService_SB.Application.DTOs;

/// <summary>
/// Información resumida del usuario autenticado.
/// </summary>
public class UserDetailsDto
{
    /// <summary>
    /// Identificador único del usuario.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Nombre de usuario.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// URL de imagen de perfil.
    /// </summary>
    public string ProfilePicture { get; set; } = string.Empty;

    /// <summary>
    /// Rol principal del usuario.
    /// </summary>
    public string Role { get; set; } = string.Empty;
}