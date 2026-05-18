using Microsoft.AspNetCore.Mvc;

namespace AuthService_SB.Api.Controllers;

/// <summary>
/// Endpoints de estado y disponibilidad del servicio.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Verifica el estado operativo del servicio de autenticación.
    /// </summary>
    /// <returns>Información básica de salud de la API.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetHealth()
    {
        var response = new
        {
            status = "Healthy",
            timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
            service = "KinalSports Authentication Service"
        };

        return Ok(response);
    }
}
