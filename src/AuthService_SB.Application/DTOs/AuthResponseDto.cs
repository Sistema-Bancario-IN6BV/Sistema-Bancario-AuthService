namespace AuthService_SB.Application.DTOs;

/// <summary>
/// Respuesta devuelta al autenticar un usuario.
/// </summary>
public class AuthResponseDto
{
    /// <summary>
    /// Indica si la operación fue exitosa.
    /// </summary>
    public bool Success { get; set; } = true;

    /// <summary>
    /// Mensaje informativo del resultado de autenticación.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Token JWT para autenticación de solicitudes.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Datos básicos del usuario autenticado.
    /// </summary>
    public UserDetailsDto UserDetails { get; set; } = new();

    /// <summary>
    /// Fecha y hora UTC de expiración del token.
    /// </summary>
    public DateTime ExpiresAt { get; set; }
}