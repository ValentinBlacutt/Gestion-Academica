using GestionApi.Data;
using GestionApi.DTOs;
using GestionApi.Enums;
using GestionApi.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionApi.Services;

public class AlumnosService : IAlumnosService
{
    private readonly AppDbContext _context;

    public AlumnosService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<AlumnoDTO>> GetActivosByCursoAsync(int cursoId)
    {
        return await _context.Alumnos
            .Where(a => a.CursoId == cursoId && a.Estado == EstadoAlumno.Activo)
            .Select(a => new AlumnoDTO
            {
                Id = a.Id,
                Nombre = a.Nombre,
                Apellido = a.Apellido,
                DNI = a.DNI,
                CursoId = a.CursoId,
                Estado = a.Estado,
                FechaBaja = a.FechaBaja
            })
            .ToListAsync();
    }

    public async Task<List<AlumnoDTO>> GetExAlumnosAsync()
    {
        return await _context.Alumnos
            .Where(a => a.Estado == EstadoAlumno.ExAlumno)
            .Select(a => new AlumnoDTO
            {
                Id = a.Id,
                Nombre = a.Nombre,
                Apellido = a.Apellido,
                DNI = a.DNI,
                CursoId = a.CursoId,
                Estado = a.Estado,
                FechaBaja = a.FechaBaja
            })
            .ToListAsync();
    }

    public async Task<List<AlumnoDTO>> GetEgresadosAsync()
    {
        return await _context.Alumnos
            .Where(a => a.Estado == EstadoAlumno.Egresado)
            .Select(a => new AlumnoDTO
            {
                Id = a.Id,
                Nombre = a.Nombre,
                Apellido = a.Apellido,
                DNI = a.DNI,
                CursoId = a.CursoId,
                Estado = a.Estado,
                FechaBaja = a.FechaBaja
            })
            .ToListAsync();
    }

    public async Task<AlumnoDTO?> GetByIdAsync(int id)
    {
        var alumno = await _context.Alumnos.FindAsync(id);
        if (alumno == null) return null;

        return new AlumnoDTO
        {
            Id = alumno.Id,
            Nombre = alumno.Nombre,
            Apellido = alumno.Apellido,
            DNI = alumno.DNI,
            CursoId = alumno.CursoId,
            Estado = alumno.Estado,
            FechaBaja = alumno.FechaBaja
        };
    }

    public async Task<AlumnoDTO> CreateAsync(AlumnoDTO dto)
    {
        var alumno = new Alumno
        {
            Nombre = dto.Nombre,
            Apellido = dto.Apellido,
            DNI = dto.DNI,
            CursoId = dto.CursoId,
            Estado = EstadoAlumno.Activo,
            FechaBaja = null
        };

        _context.Alumnos.Add(alumno);
        await _context.SaveChangesAsync();

        dto.Id = alumno.Id;
        dto.Estado = alumno.Estado;
        return dto;
    }

    public async Task<AlumnoDTO?> UpdateAsync(int id, AlumnoDTO dto)
    {
        var alumno = await _context.Alumnos.FindAsync(id);
        if (alumno == null) return null;

        alumno.Nombre = dto.Nombre;
        alumno.Apellido = dto.Apellido;
        alumno.DNI = dto.DNI;

        await _context.SaveChangesAsync();

        return new AlumnoDTO
        {
            Id = alumno.Id,
            Nombre = alumno.Nombre,
            Apellido = alumno.Apellido,
            DNI = alumno.DNI,
            CursoId = alumno.CursoId,
            Estado = alumno.Estado,
            FechaBaja = alumno.FechaBaja
        };
    }

    public async Task<bool> BajaAsync(int id)
    {
        var alumno = await _context.Alumnos.FindAsync(id);
        if (alumno == null) return false;

        alumno.Estado = EstadoAlumno.ExAlumno;
        alumno.CursoId = null;
        alumno.FechaBaja = DateTime.UtcNow.Date;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EgresarAsync(int id)
    {
        var alumno = await _context.Alumnos.FindAsync(id);
        if (alumno == null) return false;

        alumno.Estado = EstadoAlumno.Egresado;
        alumno.CursoId = null;
        alumno.FechaBaja = DateTime.UtcNow.Date;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MoverCursoAsync(int alumnoId, int cursoId)
    {
        var alumno = await _context.Alumnos.FindAsync(alumnoId);
        if (alumno == null) return false;

        var curso = await _context.Cursos.FindAsync(cursoId);
        if (curso == null) return false;

        alumno.CursoId = cursoId;
        alumno.Estado = EstadoAlumno.Activo;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<AlumnoDTO>> GetTodosAsync(int pagina, int tamanioPagina)
    {
        return await _context.Alumnos
            .Where(a => a.Estado == EstadoAlumno.Activo)
            .OrderBy(a => a.Apellido)
            .Skip((pagina - 1) * tamanioPagina)
            .Take(tamanioPagina)
            .Select(a => new AlumnoDTO
            {
                Id = a.Id,
                Nombre = a.Nombre,
                Apellido = a.Apellido,
                DNI = a.DNI,
                CursoId = a.CursoId,
                Estado = a.Estado,
                FechaBaja = a.FechaBaja
            })
            .ToListAsync();
    }
}