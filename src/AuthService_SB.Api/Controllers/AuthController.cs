using System;
using AuthService_SB.Application.DTOs;
using AuthService_SB.Application.DTOs.Email;
using AuthService_SB.Application.Interfaces;
using AuthService_SB.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AuthService_SB.Api.Controllers;

/// <summary>
/// Endpoints para autenticación, recuperación de cuenta y gestión del perfil del usuario autenticado.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary>
    /// Obtiene el perfil del usuario autenticado.
    /// </summary>
    /// <returns>Perfil actual del usuario autenticado.</returns>
    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> GetProfile()
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
        {
            return Unauthorized();
        }

        var user = await authService.GetUserByIdAsync(userIdClaim.Value);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(new
        {
            success = true,
            message = "Perfil obtenido exitosamente",
            data = user
        });
    }

    /// <summary>
    /// Registra un nuevo usuario.
    /// </summary>
    /// <param name="registerDto">Datos de registro enviados como formulario multipart (incluye imagen de perfil opcional).</param>
    /// <returns>Resultado del registro con datos del usuario creado.</returns>
    [HttpPost("register")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10MB límite
    [EnableRateLimiting("AuthPolicy")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(RegisterResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<RegisterResponseDto>> Register([FromForm] RegisterDto registerDto)
    {
        var result = await authService.RegisterAsync(registerDto);
        // Devolver 201 Created para registro
        return StatusCode(201, result);
    }

    /// <summary>
    /// Inicia sesión con correo o nombre de usuario.
    /// </summary>
    /// <param name="loginDto">Credenciales del usuario.</param>
    /// <returns>Token JWT e información del usuario.</returns>
    [HttpPost("login")]
    [EnableRateLimiting("AuthPolicy")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
    {
        var result = await authService.LoginAsync(loginDto);
        return Ok(result);
    }

    /// <summary>
    /// Verifica la cuenta de correo mediante token.
    /// </summary>
    /// <param name="verifyEmailDto">Token de verificación enviado por correo.</param>
    /// <returns>Resultado de la verificación del correo.</returns>
    [HttpPost("verify-email")]
    [EnableRateLimiting("ApiPolicy")]
    [ProducesResponseType(typeof(EmailResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<EmailResponseDto>> VerifyEmail([FromBody] VerifyEmailDto verifyEmailDto)
    {
        var result = await authService.VerifyEmailAsync(verifyEmailDto);
        return Ok(result);
    }

    /// <summary>
    /// Reenvía el correo de verificación de cuenta.
    /// </summary>
    /// <param name="resendDto">Correo electrónico del usuario a verificar.</param>
    /// <returns>Resultado del envío del correo de verificación.</returns>
    [HttpPost("resend-verification")]
    [EnableRateLimiting("AuthPolicy")]
    [ProducesResponseType(typeof(EmailResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EmailResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(EmailResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EmailResponseDto), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(EmailResponseDto), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<EmailResponseDto>> ResendVerification([FromBody] ResendVerificationDto resendDto)
    {
        var result = await authService.ResendVerificationEmailAsync(resendDto);

        // Return appropriate status code based on result
        if (!result.Success)
        {
            if (result.Message.Contains("no encontrado", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(result);
            }
            if (result.Message.Contains("ya ha sido verificado", StringComparison.OrdinalIgnoreCase) ||
                result.Message.Contains("ya verificado", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(result);
            }
            // Email sending failed - Service Unavailable
            return StatusCode(503, result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Inicia el proceso de recuperación de contraseña.
    /// </summary>
    /// <param name="forgotPasswordDto">Correo electrónico asociado a la cuenta.</param>
    /// <returns>Resultado del proceso de recuperación.</returns>
    [HttpPost("forgot-password")]
    [EnableRateLimiting("AuthPolicy")]
    [ProducesResponseType(typeof(EmailResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EmailResponseDto), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(EmailResponseDto), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<EmailResponseDto>> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
    {
        var result = await authService.ForgotPasswordAsync(forgotPasswordDto);

        // ForgotPassword always returns success for security (even if user not found)
        // But if email sending fails, return 503
        if (!result.Success)
        {
            return StatusCode(503, result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Restablece la contraseña usando token de recuperación.
    /// </summary>
    /// <param name="resetPasswordDto">Token de recuperación y nueva contraseña.</param>
    /// <returns>Resultado del restablecimiento de contraseña.</returns>
    [HttpPost("reset-password")]
    [EnableRateLimiting("AuthPolicy")]
    [ProducesResponseType(typeof(EmailResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<EmailResponseDto>> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
    {
        var result = await authService.ResetPasswordAsync(resetPasswordDto);
        return Ok(result);
    }

    /// <summary>
    /// Cambia la contraseña del usuario autenticado.
    /// </summary>
    /// <param name="changePasswordDto">Contraseña actual y nueva contraseña con confirmación.</param>
    /// <returns>Estado del cambio de contraseña.</returns>
    [HttpPost("change-password")]
    [Authorize]
    [EnableRateLimiting("ApiPolicy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<object>> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
        {
            return Unauthorized();
        }

        // Validar que las contraseñas coincidan
        if (changePasswordDto.NewPassword != changePasswordDto.ConfirmPassword)
        {
            return BadRequest(new
            {
                success = false,
                message = "Las contraseñas nuevas no coinciden"
            });
        }

        var result = await authService.ChangePasswordAsync(userIdClaim.Value, changePasswordDto);
        if (!result)
        {
            return BadRequest(new
            {
                success = false,
                message = "Error al cambiar la contraseña"
            });
        }

        return Ok(new
        {
            success = true,
            message = "Contraseña actualizada exitosamente"
        });
    }

    /// <summary>
    /// Actualiza el perfil completo del usuario autenticado.
    /// </summary>
    /// <param name="updateUserProfileDto">Campos editables del perfil enviados como formulario multipart.</param>
    /// <returns>Perfil actualizado del usuario.</returns>
    [HttpPut("profile")]
    [Authorize]
    [RequestSizeLimit(10 * 1024 * 1024)]
    [EnableRateLimiting("ApiPolicy")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<object>> UpdateProfile([FromForm] UpdateUserProfileDto updateUserProfileDto)
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
        {
            return Unauthorized();
        }

        var user = await authService.UpdateUserProfileAsync(userIdClaim.Value, updateUserProfileDto);
        if (user == null)
        {
            return BadRequest(new
            {
                success = false,
                message = "Error al actualizar el perfil"
            });
        }

        return Ok(new
        {
            success = true,
            message = "Perfil actualizado exitosamente",
            data = user
        });
    }

    /// <summary>
    /// Actualiza los datos de perfil de cliente del usuario autenticado.
    /// </summary>
    /// <param name="updateClientProfileDto">Datos de cliente a modificar.</param>
    /// <returns>Perfil de cliente actualizado.</returns>
    [HttpPut("client-profile")]
    [Authorize]
    [EnableRateLimiting("ApiPolicy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<object>> UpdateClientProfile([FromBody] UpdateClientProfileDto updateClientProfileDto)
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
        {
            return Unauthorized();
        }

        var user = await authService.UpdateClientProfileAsync(userIdClaim.Value, updateClientProfileDto);
        if (user == null)
        {
            return BadRequest(new
            {
                success = false,
                message = "Error al actualizar el perfil"
            });
        }

        return Ok(new
        {
            success = true,
            message = "Perfil del cliente actualizado exitosamente",
            data = user
        });
    }
}
