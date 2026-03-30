namespace GestionApiClient.DTOs;

public class AlumnoDTO
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string DNI { get; set; } = string.Empty;
    public int? CursoId { get; set; }
    public int Estado { get; set; } 
    public DateTime? FechaBaja { get; set; }
}