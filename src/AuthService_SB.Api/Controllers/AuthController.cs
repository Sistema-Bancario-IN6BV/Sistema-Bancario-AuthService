using System;
using AuthService_SB.Application.DTOs;
using AuthService_SB.Application.DTOs.Email;
using AuthService_SB.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AuthService_SB.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpGet("profile")]
    [Authorize]
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
    [HttpPost("register")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10MB límite
    [EnableRateLimiting("AuthPolicy")]
    public async Task<ActionResult<RegisterResponseDto>> Register([FromForm] RegisterDto registerDto)
    {
        var result = await authService.RegisterAsync(registerDto);
        // Devolver 201 Created para registro
        return StatusCode(201, result);
    }

    [HttpPost("login")]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
    {
        var result = await authService.LoginAsync(loginDto);
        return Ok(result);
    }

    [HttpPost("verify-email")]
    [EnableRateLimiting("ApiPolicy")]
    public async Task<ActionResult<EmailResponseDto>> VerifyEmail([FromBody] VerifyEmailDto verifyEmailDto)
    {
        var result = await authService.VerifyEmailAsync(verifyEmailDto);
        return Ok(result);
    }

    [HttpPost("resend-verification")]
    [EnableRateLimiting("AuthPolicy")]
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

    [HttpPost("forgot-password")]
    [EnableRateLimiting("AuthPolicy")]
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

    [HttpPost("reset-password")]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<ActionResult<EmailResponseDto>> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
    {
        var result = await authService.ResetPasswordAsync(resetPasswordDto);
        return Ok(result);
    }

    [HttpPost("change-password")]
    [Authorize]
    [EnableRateLimiting("ApiPolicy")]
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

    [HttpPut("profile")]
    [Authorize]
    [RequestSizeLimit(10 * 1024 * 1024)]
    [EnableRateLimiting("ApiPolicy")]
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

    [HttpPut("client-profile")]
    [Authorize]
    [EnableRateLimiting("ApiPolicy")]
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
