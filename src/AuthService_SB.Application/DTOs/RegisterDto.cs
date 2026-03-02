
using System.ComponentModel.DataAnnotations;
using AuthService_SB.Application.Interfaces;

namespace AuthService_SB.Application.DTOs;

public class RegisterDto
{
    [Required]
    [MaxLength(25)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(25)]
    public string Surname { get; set; } = string.Empty;

    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [StringLength(8, MinimumLength = 8)]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^\d{13}$", ErrorMessage = "El DPI debe contener exactamente 13 dígitos")]
    public string Dpi { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Address { get; set; } = string.Empty;

    [MaxLength(100)]
    public string JobName { get; set; } = string.Empty;

    [Required]
    public decimal MonthlyIncome { get; set; }

    public IFileData? ProfilePicture { get; set; }
}