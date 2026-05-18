using System.ComponentModel.DataAnnotations;

namespace AuthService_SB.Application.DTOs.Email;

/// <summary>
/// Datos para solicitar recuperación de contraseña.
/// </summary>
public class ForgotPasswordDto
{
    /// <summary>
    /// Correo electrónico de la cuenta.
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}