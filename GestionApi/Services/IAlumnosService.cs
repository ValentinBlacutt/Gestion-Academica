using GestionApi.DTOs;

namespace GestionApi.Services;

public interface IAlumnosService
{
    Task<List<AlumnoDTO>> GetActivosByCursoAsync(int cursoId);
    Task<List<AlumnoDTO>> GetExAlumnosAsync();
    Task<List<AlumnoDTO>> GetEgresadosAsync();
    Task<AlumnoDTO?> GetByIdAsync(int id);
    Task<AlumnoDTO> CreateAsync(AlumnoDTO dto);
    Task<AlumnoDTO?> UpdateAsync(int id, AlumnoDTO dto);
    Task<bool> BajaAsync(int id);
    Task<bool> EgresarAsync(int id);
    Task<bool> MoverCursoAsync(int alumnoId, int cursoId);

    Task<List<AlumnoDTO>> GetTodosAsync(int pagina, int tamanioPagina);
}
