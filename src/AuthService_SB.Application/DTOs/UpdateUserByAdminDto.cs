using System.ComponentModel.DataAnnotations;
using AuthService_SB.Application.Interfaces;

namespace AuthService_SB.Application.DTOs;

public class UpdateUserByAdminDto
{
    [MaxLength(25, ErrorMessage = "El nombre no puede tener más de 25 caracteres.")]
    public string? Name { get; set; }

    [MaxLength(25, ErrorMessage = "El apellido no puede tener más de 25 caracteres.")]
    public string? Surname { get; set; }

    [EmailAddress(ErrorMessage = "El correo debe ser válido.")]
    public string? Email { get; set; }

    [StringLength(8, MinimumLength = 8, ErrorMessage = "El teléfono debe tener exactamente 8 dígitos")]
    public string? Phone { get; set; }

    [MaxLength(255, ErrorMessage = "La dirección no puede tener más de 255 caracteres.")]
    public string? Address { get; set; }

    [MaxLength(100, ErrorMessage = "El nombre del trabajo no puede tener más de 100 caracteres.")]
    public string? JobName { get; set; }

    [Range(100, double.MaxValue, ErrorMessage = "Los ingresos mensuales deben ser mayor a Q100.")]
    public decimal? MonthlyIncome { get; set; }

    public IFileData? ProfilePicture { get; set; }

    // Nota: El admin NO puede cambiar el DPI ni la contraseña según las instrucciones
}
