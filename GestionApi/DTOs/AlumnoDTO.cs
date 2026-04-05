using GestionApi.Enums;
using System.ComponentModel.DataAnnotations;

namespace GestionApi.DTOs;

public class AlumnoDTO
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El nombre solo puede contener letras")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El apellido solo puede contener letras")]
    public string Apellido { get; set; } = string.Empty;

    [Required(ErrorMessage = "El DNI es obligatorio")]
    [RegularExpression(@"^\d{1,8}$", ErrorMessage = "El DNI debe contener solo números y tener máximo 8 dígitos")]
    public string DNI { get; set; } = string.Empty;

    public int? CursoId { get; set; }
    public EstadoAlumno Estado { get; set; }
    public DateTime? FechaBaja { get; set; }
}