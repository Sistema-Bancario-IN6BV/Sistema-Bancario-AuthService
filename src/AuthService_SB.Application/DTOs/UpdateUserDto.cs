namespace AuthService_SB.Application.DTOs;

/// <summary>
/// Datos básicos para actualizar usuario.
/// </summary>
public class UpdateUserDto
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
}
