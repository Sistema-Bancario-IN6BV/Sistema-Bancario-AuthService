using System.ComponentModel.DataAnnotations;

namespace AuthService_SB.Application.DTOs;

public class UpdateClientProfileDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(25, ErrorMessage = "El nombre no puede tener más de 25 caracteres.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [MaxLength(25, ErrorMessage = "El apellido no puede tener más de 25 caracteres.")]
    public string Surname { get; set; } = string.Empty;

    [Required(ErrorMessage = "La dirección es obligatoria.")]
    [MaxLength(255, ErrorMessage = "La dirección no puede tener más de 255 caracteres.")]
    public string Address { get; set; } = string.Empty;

    [MaxLength(100, ErrorMessage = "El nombre del trabajo no puede tener más de 100 caracteres.")]
    public string JobName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los ingresos mensuales son obligatorios.")]
    [Range(100, double.MaxValue, ErrorMessage = "Los ingresos mensuales deben ser mayor a Q100.")]
    public decimal MonthlyIncome { get; set; }
}
