using System.ComponentModel.DataAnnotations;

namespace AuthService_SB.Application.DTOs.Email;

/// <summary>
/// Datos para verificar una cuenta de correo.
/// </summary>
public class VerifyEmailDto
{
    /// <summary>
    /// Token de verificación del correo.
    /// </summary>
    [Required]
    public string Token { get; set; } = string.Empty;
}