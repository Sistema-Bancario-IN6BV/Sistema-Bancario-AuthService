using System.ComponentModel.DataAnnotations;

namespace AuthService_SB.Application.DTOs.Email;

public class VerifyEmailDto
{
    [Required]
    public string Token { get; set; } = string.Empty;
}