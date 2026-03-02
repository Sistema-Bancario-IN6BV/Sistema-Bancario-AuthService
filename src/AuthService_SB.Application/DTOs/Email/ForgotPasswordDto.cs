using System.ComponentModel.DataAnnotations;

namespace AuthService_SB.Application.DTOs.Email;

public class ForgotPasswordDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}