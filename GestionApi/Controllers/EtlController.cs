using GestionApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionApi.Controllers;

[ApiController]
[Route("api/etl")]
[Authorize(Roles = "Admin,Directivo")]
public class EtlController : ControllerBase
{
    private readonly IEtlService _etlService;

    public EtlController(IEtlService etlService)
    {
        _etlService = etlService;
    }

    [HttpGet("exportar/alumnos/{cursoId}")]
    public async Task<IActionResult> ExportarAlumnos(int cursoId)
    {
        var archivo = await _etlService.ExportarAlumnosAsync(cursoId);
        return File(archivo, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"alumnos_curso_{cursoId}.xlsx");
    }

    [HttpPost("importar/alumnos")]
    public async Task<IActionResult> ImportarAlumnos(IFormFile archivo)
    {
        if (archivo == null || archivo.Length == 0)
            return BadRequest("No se envió ningún archivo.");

        using var stream = new MemoryStream();
        await archivo.CopyToAsync(stream);

        var mensajes = await _etlService.ImportarAlumnosAsync(stream.ToArray());
        return Ok(mensajes);
    }
}