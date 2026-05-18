
using System.ComponentModel.DataAnnotations;
using AuthService_SB.Application.Interfaces;

namespace AuthService_SB.Application.DTOs;

/// <summary>
/// Datos requeridos para registrar una cuenta nueva.
/// </summary>
public class RegisterDto
{
    /// <summary>
    /// Nombre del usuario.
    /// </summary>
    [Required]
    [MaxLength(25)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Apellido del usuario.
    /// </summary>
    [Required]
    [MaxLength(25)]
    public string Surname { get; set; } = string.Empty;

    /// <summary>
    /// Nombre de usuario único.
    /// </summary>
    [Required]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Correo electrónico del usuario.
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Contraseña de la cuenta (mínimo 8 caracteres).
    /// </summary>
    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Teléfono de 8 dígitos.
    /// </summary>
    [Required]
    [StringLength(8, MinimumLength = 8)]
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Documento personal de identificación (13 dígitos).
    /// </summary>
    [Required]
    [RegularExpression(@"^\d{13}$", ErrorMessage = "El DPI debe contener exactamente 13 dígitos")]
    public string Dpi { get; set; } = string.Empty;

    /// <summary>
    /// Dirección de residencia del usuario.
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Puesto o nombre del trabajo.
    /// </summary>
    [MaxLength(100)]
    public string JobName { get; set; } = string.Empty;

    /// <summary>
    /// Ingresos mensuales del usuario.
    /// </summary>
    [Required]
    public decimal MonthlyIncome { get; set; }

    /// <summary>
    /// Imagen de perfil opcional enviada como archivo.
    /// </summary>
    public IFileData? ProfilePicture { get; set; }
}