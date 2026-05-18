using System.Diagnostics;

namespace AuthService_SB.Api.Models;

/// <summary>
/// Estructura estándar para errores devueltos por la API.
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Código HTTP del error.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Título corto del error.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Descripción detallada del error.
    /// </summary>
    public string Detail { get; set; } = string.Empty;

    /// <summary>
    /// Código de error de negocio opcional.
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Identificador de rastreo para correlación de logs.
    /// </summary>
    public string TraceId { get; set; } = Activity.Current?.Id ?? string.Empty;

    /// <summary>
    /// Fecha y hora UTC en la que se generó el error.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}