using System.ComponentModel.DataAnnotations;
using AuthService_SB.Application.Interfaces;

namespace AuthService_SB.Application.DTOs;

public class CreateUserByAdminDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(25, ErrorMessage = "El nombre no puede tener más de 25 caracteres.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [MaxLength(25, ErrorMessage = "El apellido no puede tener más de 25 caracteres.")]
    public string Surname { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [MaxLength(25)]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo debe ser válido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [StringLength(8, MinimumLength = 8, ErrorMessage = "El teléfono debe tener exactamente 8 dígitos")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "El DPI es obligatorio.")]
    [RegularExpression(@"^\d{13}$", ErrorMessage = "El DPI debe contener exactamente 13 dígitos")]
    public string Dpi { get; set; } = string.Empty;

    [Required(ErrorMessage = "La dirección es obligatoria.")]
    [MaxLength(255, ErrorMessage = "La dirección no puede tener más de 255 caracteres.")]
    public string Address { get; set; } = string.Empty;

    [MaxLength(100, ErrorMessage = "El nombre del trabajo no puede tener más de 100 caracteres.")]
    public string JobName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los ingresos mensuales son obligatorios.")]
    [Range(100, double.MaxValue, ErrorMessage = "Los ingresos mensuales deben ser mayor a Q100.")]
    public decimal MonthlyIncome { get; set; }

    [Required(ErrorMessage = "El rol es obligatorio.")]
    public string RoleName { get; set; } = string.Empty; // "Admin" o "Client"

    public IFileData? ProfilePicture { get; set; }
}
