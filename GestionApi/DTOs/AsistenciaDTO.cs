using GestionApi.Enums;

namespace GestionApi.DTOs;

public class AsistenciaDTO
{
    public int Id { get; set; }
    public int AlumnoId { get; set; }
    public int CursoId { get; set; }
    public DateOnly Fecha { get; set; }
    public EstadoAsistencia Estado { get; set; }
}