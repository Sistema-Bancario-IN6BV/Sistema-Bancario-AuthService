using System.ComponentModel.DataAnnotations;

namespace AuthService_SB.Application.DTOs.Email;

/// <summary>
/// Datos para reenviar correo de verificación.
/// </summary>
public class ResendVerificationDto
{
    /// <summary>
    /// Correo electrónico de la cuenta.
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}