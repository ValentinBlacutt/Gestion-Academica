using GestionApi.DTOs;
using GestionApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace GestionApi.Controllers;

[ApiController]
[Route("api/cursos")]
public class CursosController : ControllerBase
{
    private readonly ICursosService _cursosService;

    public CursosController(ICursosService cursosService)
    {
        _cursosService = cursosService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var cursos = await _cursosService.GetAllAsync();
        return Ok(cursos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var curso = await _cursosService.GetByIdAsync(id);
        if (curso == null) return NotFound();
        return Ok(curso);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CursoDTO dto)
    {
        var creado = await _cursosService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CursoDTO dto)
    {
        var actualizado = await _cursosService.UpdateAsync(id, dto);
        if (actualizado == null) return NotFound();
        return Ok(actualizado);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var resultado = await _cursosService.DeleteAsync(id);
        if (!resultado) return BadRequest("No se puede eliminar el curso. Puede que no exista o tenga alumnos activos.");
        return NoContent();
    }
}