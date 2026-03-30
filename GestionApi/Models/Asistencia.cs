using GestionApi.Enums;

namespace GestionApi.Models;

public class Asistencia
{
    public int Id { get; set; }

    public int AlumnoId { get; set; }
    public Alumno Alumno { get; set; } = null!;

    public int CursoId { get; set; }
    public Curso Curso { get; set; } = null!;

    public DateOnly Fecha { get; set; }
    public EstadoAsistencia Estado { get; set; }
}