using GestionApi.Data;
using GestionApi.DTOs;
using GestionApi.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionApi.Services;

public class CursosService : ICursosService
{
    private readonly AppDbContext _context;

    public CursosService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CursoDTO>> GetAllAsync()
    {
        return await _context.Cursos
            .Select(c => new CursoDTO
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Anio = c.Anio,
                Division = c.Division
            })
            .ToListAsync();
    }

    public async Task<CursoDTO?> GetByIdAsync(int id)
    {
        var curso = await _context.Cursos.FindAsync(id);
        if (curso == null) return null;

        return new CursoDTO
        {
            Id = curso.Id,
            Nombre = curso.Nombre,
            Anio = curso.Anio,
            Division = curso.Division
        };
    }

    public async Task<CursoDTO> CreateAsync(CursoDTO dto)
    {
        var curso = new Curso
        {
            Nombre = dto.Nombre,
            Anio = dto.Anio,
            Division = dto.Division
        };

        _context.Cursos.Add(curso);
        await _context.SaveChangesAsync();

        dto.Id = curso.Id;
        return dto;
    }

    public async Task<CursoDTO?> UpdateAsync(int id, CursoDTO dto)
    {
        var curso = await _context.Cursos.FindAsync(id);
        if (curso == null) return null;

        curso.Nombre = dto.Nombre;
        curso.Anio = dto.Anio;
        curso.Division = dto.Division;

        await _context.SaveChangesAsync();

        return new CursoDTO
        {
            Id = curso.Id,
            Nombre = curso.Nombre,
            Anio = curso.Anio,
            Division = curso.Division
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var curso = await _context.Cursos
            .Include(c => c.Alumnos)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (curso == null) return false;

        // Regla de negocio: no se puede eliminar si tiene alumnos activos
        bool tieneAlumnosActivos = curso.Alumnos
            .Any(a => a.Estado == Enums.EstadoAlumno.Activo);

        if (tieneAlumnosActivos) return false;

        _context.Cursos.Remove(curso);
        await _context.SaveChangesAsync();
        return true;
    }
}