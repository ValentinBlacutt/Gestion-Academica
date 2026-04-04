using GestionApi.DTOs;
using GestionApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionApi.Controllers;

[ApiController]
[Route("api/asistencias")]
[Authorize]
public class AsistenciasController : ControllerBase
{
    private readonly IAsistenciasService _asistenciasService;

    public AsistenciasController(IAsistenciasService asistenciasService)
    {
        _asistenciasService = asistenciasService;
    }

    [HttpGet("curso/{id}/hoy")]
    public async Task<IActionResult> GetByCursoHoy(int id)
    {
        var asistencias = await _asistenciasService.GetByCursoHoyAsync(id);
        return Ok(asistencias);
    }

    [HttpGet("alumno/{id}/resumen")]
    public async Task<IActionResult> GetResumenAlumno(int id)
    {
        var resumen = await _asistenciasService.GetResumenAlumnoAsync(id);
        return Ok(resumen);
    }

    [HttpPost("bulk")]
    public async Task<IActionResult> BulkCreate(List<AsistenciaDTO> dtos)
    {
        await _asistenciasService.BulkCreateAsync(dtos);
        return NoContent();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, AsistenciaDTO dto)
    {
        var actualizado = await _asistenciasService.UpdateAsync(id, dto);
        if (actualizado == null) return NotFound();
        return Ok(actualizado);
    }
}