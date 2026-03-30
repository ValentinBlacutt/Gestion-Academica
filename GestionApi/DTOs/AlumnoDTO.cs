using GestionApi.Enums;

namespace GestionApi.DTOs;

public class AlumnoDTO
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string DNI { get; set; } = string.Empty;
    public int? CursoId { get; set; }
    public EstadoAlumno Estado { get; set; }
    public DateTime? FechaBaja { get; set; }
}