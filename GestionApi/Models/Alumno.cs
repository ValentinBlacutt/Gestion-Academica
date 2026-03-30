using GestionApi.Enums;

namespace GestionApi.Models;

public class Alumno
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string DNI { get; set; } = string.Empty;

    public int? CursoId { get; set; }
    public Curso? Curso { get; set; }

    public EstadoAlumno Estado { get; set; } = EstadoAlumno.Activo;
    public DateTime? FechaBaja { get; set; }

    // Navegación
    public ICollection<Asistencia> Asistencias { get; set; } = new List<Asistencia>();
}