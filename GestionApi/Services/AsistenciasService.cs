using GestionApi.Data;
using GestionApi.DTOs;
using GestionApi.Enums;
using GestionApi.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionApi.Services;

public class AsistenciasService : IAsistenciasService
{
    private readonly AppDbContext _context;

    public AsistenciasService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<AsistenciaDTO>> GetByCursoHoyAsync(int cursoId)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);

        return await _context.Asistencias
            .Where(a => a.CursoId == cursoId && a.Fecha == hoy)
            .Select(a => new AsistenciaDTO
            {
                Id = a.Id,
                AlumnoId = a.AlumnoId,
                CursoId = a.CursoId,
                Fecha = a.Fecha,
                Estado = a.Estado
            })
            .ToListAsync();
    }

    public async Task<Dictionary<string, int>> GetResumenAlumnoAsync(int alumnoId)
    {
        var asistencias = await _context.Asistencias
            .Where(a => a.AlumnoId == alumnoId)
            .ToListAsync();

        return new Dictionary<string, int>
        {
            { "Presente",            asistencias.Count(a => a.Estado == EstadoAsistencia.Presente) },
            { "Ausente",             asistencias.Count(a => a.Estado == EstadoAsistencia.Ausente) },
            { "Tarde",               asistencias.Count(a => a.Estado == EstadoAsistencia.Tarde) },
            { "AusenteConPresencia", asistencias.Count(a => a.Estado == EstadoAsistencia.AusenteConPresencia) },
            { "AusenteJustificado",  asistencias.Count(a => a.Estado == EstadoAsistencia.AusenteJustificado) }
        };
    }

    public async Task BulkCreateAsync(List<AsistenciaDTO> dtos)
    {
        var asistencias = dtos.Select(dto => new Asistencia
        {
            AlumnoId = dto.AlumnoId,
            CursoId = dto.CursoId,
            Fecha = dto.Fecha,
            Estado = dto.Estado
        }).ToList();

        _context.Asistencias.AddRange(asistencias);
        await _context.SaveChangesAsync();
    }

    public async Task<AsistenciaDTO?> UpdateAsync(int id, AsistenciaDTO dto)
    {
        var asistencia = await _context.Asistencias.FindAsync(id);
        if (asistencia == null) return null;

        asistencia.Estado = dto.Estado;

        await _context.SaveChangesAsync();

        return new AsistenciaDTO
        {
            Id = asistencia.Id,
            AlumnoId = asistencia.AlumnoId,
            CursoId = asistencia.CursoId,
            Fecha = asistencia.Fecha,
            Estado = asistencia.Estado
        };
    }
}