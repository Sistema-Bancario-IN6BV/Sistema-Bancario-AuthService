namespace AuthService_SB.Application.DTOs.Email;

/// <summary>
/// Respuesta genérica para operaciones relacionadas con correo electrónico.
/// </summary>
public class EmailResponseDto
{
    /// <summary>
    /// Indica si la operación fue exitosa.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Mensaje descriptivo del resultado.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Datos adicionales opcionales.
    /// </summary>
    public object? Data { get; set; }
}