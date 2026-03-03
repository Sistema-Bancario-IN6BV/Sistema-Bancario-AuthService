namespace AuthService_SB.Application.DTOs;

/// <summary>
/// Datos para cambiar contraseña del usuario autenticado.
/// </summary>
public class ChangePasswordDto
{
    /// <summary>
    /// Contraseña actual del usuario.
    /// </summary>
    public string CurrentPassword { get; set; } = string.Empty;

    /// <summary>
    /// Nueva contraseña.
    /// </summary>
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// Confirmación de la nueva contraseña.
    /// </summary>
    public string ConfirmPassword { get; set; } = string.Empty;
}
