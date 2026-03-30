using GestionApi.DTOs;

namespace GestionApi.Services;

public interface IAsistenciasService
{
    Task<List<AsistenciaDTO>> GetByCursoHoyAsync(int cursoId);
    Task<Dictionary<string, int>> GetResumenAlumnoAsync(int alumnoId);
    Task BulkCreateAsync(List<AsistenciaDTO> dtos);
    Task<AsistenciaDTO?> UpdateAsync(int id, AsistenciaDTO dto);
}