using System.ComponentModel.DataAnnotations;

namespace AuthService_SB.Application.DTOs.Email;

/// <summary>
/// Datos para restablecer contraseña mediante token.
/// </summary>
public class ResetPasswordDto
{
    /// <summary>
    /// Token de recuperación enviado por correo.
    /// </summary>
    public string Token { get; set; } = string.Empty;
    
    /// <summary>
    /// Nueva contraseña para la cuenta.
    /// </summary>
    [Required]
    [MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;
}