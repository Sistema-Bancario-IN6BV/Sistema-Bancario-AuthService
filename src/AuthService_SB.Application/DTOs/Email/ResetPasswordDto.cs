using System.ComponentModel.DataAnnotations;

namespace AuthService_SB.Application.DTOs.Email;

public class ResetPasswordDto
{
    public string Token { get; set; } = string.Empty;
    
    [Required]
    [MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;
}