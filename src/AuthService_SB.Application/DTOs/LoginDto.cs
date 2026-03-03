using System.ComponentModel.DataAnnotations;

namespace AuthService_SB.Application.DTOs;

/// <summary>
/// Credenciales para iniciar sesión.
/// </summary>
public class LoginDto
{
    /// <summary>
    /// Correo electrónico o nombre de usuario.
    /// </summary>
    [Required]
    public string EmailOrUsername { get; set; } = string.Empty;

    /// <summary>
    /// Contraseña de acceso.
    /// </summary>
    [Required]
    public string Password { get; set; } = string.Empty;
}