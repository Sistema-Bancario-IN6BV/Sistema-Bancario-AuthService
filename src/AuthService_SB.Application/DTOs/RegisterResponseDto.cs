namespace AuthService_SB.Application.DTOs;

/// <summary>
/// Respuesta del endpoint de registro.
/// </summary>
public class RegisterResponseDto
{
    /// <summary>
    /// Indica si el registro fue exitoso.
    /// </summary>
    public bool Success { get; set; } = false;

    /// <summary>
    /// Usuario registrado.
    /// </summary>
    public UserResponseDto User { get; set; } = new();

    /// <summary>
    /// Mensaje descriptivo del resultado.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Indica si el usuario debe verificar su correo.
    /// </summary>
    public bool EmailVerificationRequired { get; set; } = true;
}