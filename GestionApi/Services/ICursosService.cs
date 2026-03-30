using GestionApi.DTOs;

namespace GestionApi.Services;

public interface ICursosService
{
    Task<List<CursoDTO>> GetAllAsync();
    Task<CursoDTO?> GetByIdAsync(int id);
    Task<CursoDTO> CreateAsync(CursoDTO dto);
    Task<CursoDTO?> UpdateAsync(int id, CursoDTO dto);
    Task<bool> DeleteAsync(int id);
}