namespace GestionApi.Models;

public class Curso
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Anio { get; set; }
    public string Division { get; set; } = string.Empty;

    // Navegación
    public ICollection<Alumno> Alumnos { get; set; } = new List<Alumno>();
    public ICollection<Asistencia> Asistencias { get; set; } = new List<Asistencia>();
}