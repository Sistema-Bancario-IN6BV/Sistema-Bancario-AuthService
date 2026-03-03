using System.ComponentModel.DataAnnotations;
using AuthService_SB.Application.Interfaces;

namespace AuthService_SB.Application.DTOs;

/// <summary>
/// Campos permitidos para que un administrador actualice un usuario.
/// </summary>
public class UpdateUserByAdminDto
{
    /// <summary>
    /// Nombre del usuario.
    /// </summary>
    [MaxLength(25, ErrorMessage = "El nombre no puede tener más de 25 caracteres.")]
    public string? Name { get; set; }

    /// <summary>
    /// Apellido del usuario.
    /// </summary>
    [MaxLength(25, ErrorMessage = "El apellido no puede tener más de 25 caracteres.")]
    public string? Surname { get; set; }

    /// <summary>
    /// Correo electrónico del usuario.
    /// </summary>
    [EmailAddress(ErrorMessage = "El correo debe ser válido.")]
    public string? Email { get; set; }

    /// <summary>
    /// Teléfono de 8 dígitos.
    /// </summary>
    [StringLength(8, MinimumLength = 8, ErrorMessage = "El teléfono debe tener exactamente 8 dígitos")]
    public string? Phone { get; set; }

    /// <summary>
    /// Dirección del usuario.
    /// </summary>
    [MaxLength(255, ErrorMessage = "La dirección no puede tener más de 255 caracteres.")]
    public string? Address { get; set; }

    /// <summary>
    /// Nombre del trabajo o puesto laboral.
    /// </summary>
    [MaxLength(100, ErrorMessage = "El nombre del trabajo no puede tener más de 100 caracteres.")]
    public string? JobName { get; set; }

    /// <summary>
    /// Ingreso mensual del usuario.
    /// </summary>
    [Range(100, double.MaxValue, ErrorMessage = "Los ingresos mensuales deben ser mayor a Q100.")]
    public decimal? MonthlyIncome { get; set; }

    /// <summary>
    /// Imagen de perfil opcional enviada como archivo.
    /// </summary>
    public IFileData? ProfilePicture { get; set; }

    // Nota: El admin NO puede cambiar el DPI ni la contraseña según las instrucciones
}
