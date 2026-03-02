using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthService_SB.Domain.Entities;
public class UserProfile
{
    [Key]
    [MaxLength(16)]
    public string Id { get; set;} = string.Empty;

    [Required]
    [MaxLength(16)]
    public string UserId { get; set; } = string.Empty;

    [MaxLength(512)]
    public string ProfilePicture { get; set; } = string.Empty;

    [Required(ErrorMessage = "El número de teléfono es obligatorio.")]
    [StringLength(8, MinimumLength = 8, ErrorMessage = "El número de teléfono debe tener exactamente 8 dígitos")]
    [RegularExpression(@"^\d{8}$", ErrorMessage = "El teléfono solo debe contener números")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "El DPI es obligatorio.")]
    [MaxLength(13, ErrorMessage = "El DPI no puede tener más de 13 caracteres.")]
    [RegularExpression(@"^\d{13}$", ErrorMessage = "El DPI debe contener exactamente 13 dígitos")]
    public string Dpi { get; set; } = string.Empty;

    [Required(ErrorMessage = "La dirección es obligatoria.")]
    [MaxLength(255, ErrorMessage = "La dirección no puede tener más de 255 caracteres.")]
    public string Address { get; set; } = string.Empty;

    [MaxLength(100, ErrorMessage = "El nombre del trabajo no puede tener más de 100 caracteres.")]
    public string JobName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los ingresos mensuales son obligatorios.")]
    [Column(TypeName = "decimal(12, 2)")]
    public decimal MonthlyIncome { get; set; }

    [Required]
    public User User { get; set; } = null!;
}