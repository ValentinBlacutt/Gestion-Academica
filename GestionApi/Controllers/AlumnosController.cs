using GestionApi.DTOs;
using GestionApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionApi.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class AlumnosController : ControllerBase
{
    private readonly IAlumnosService _alumnosService;

    public AlumnosController(IAlumnosService alumnosService)
    {
        _alumnosService = alumnosService;
    }

    [HttpGet("cursos/{id}/alumnos")]
    public async Task<IActionResult> GetActivosByCurso(int id)
    {
        var alumnos = await _alumnosService.GetActivosByCursoAsync(id);
        return Ok(alumnos);
    }

    [HttpGet("alumnos/exalumnos")]
    public async Task<IActionResult> GetExAlumnos()
    {
        var alumnos = await _alumnosService.GetExAlumnosAsync();
        return Ok(alumnos);
    }

    [HttpGet("alumnos/egresados")]
    public async Task<IActionResult> GetEgresados()
    {
        var alumnos = await _alumnosService.GetEgresadosAsync();
        return Ok(alumnos);
    }

    [HttpGet("alumnos/{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var alumno = await _alumnosService.GetByIdAsync(id);
        if (alumno == null) return NotFound();
        return Ok(alumno);
    }

    [HttpPost("alumnos")]
    [Authorize(Roles = "Admin,Directivo")]
    public async Task<IActionResult> Create(AlumnoDTO dto)
    {
        var creado = await _alumnosService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
    }

    [HttpPut("alumnos/{id}")]
    [Authorize(Roles = "Admin,Directivo")]
    public async Task<IActionResult> Update(int id, AlumnoDTO dto)
    {
        var actualizado = await _alumnosService.UpdateAsync(id, dto);
        if (actualizado == null) return NotFound();
        return Ok(actualizado);
    }

    [HttpPatch("alumnos/{id}/baja")]
    [Authorize(Roles = "Admin,Directivo")]
    public async Task<IActionResult> Baja(int id)
    {
        var resultado = await _alumnosService.BajaAsync(id);
        if (!resultado) return NotFound();
        return NoContent();
    }

    [HttpPatch("alumnos/{id}/egresar")]
    [Authorize(Roles = "Admin,Directivo")]
    public async Task<IActionResult> Egresar(int id)
    {
        var resultado = await _alumnosService.EgresarAsync(id);
        if (!resultado) return NotFound();
        return NoContent();
    }

    [HttpPatch("alumnos/{alumnoId}/mover/{cursoId}")]
    [Authorize(Roles = "Admin,Directivo")]
    public async Task<IActionResult> MoverCurso(int alumnoId, int cursoId)
    {
        var resultado = await _alumnosService.MoverCursoAsync(alumnoId, cursoId);
        if (!resultado) return NotFound();
        return NoContent();
    }

    [HttpGet("alumnos")]
    public async Task<IActionResult> GetTodos([FromQuery] int pagina = 1, [FromQuery] int tamanioPagina = 10)
    {
        var alumnos = await _alumnosService.GetTodosAsync(pagina, tamanioPagina);
        return Ok(alumnos);
    }
}