using ClosedXML.Excel;
using GestionApi.Data;
using GestionApi.Enums;
using Microsoft.EntityFrameworkCore;

namespace GestionApi.Services;

public class EtlService : IEtlService
{
    private readonly AppDbContext _context;

    public EtlService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<byte[]> ExportarAlumnosAsync(int cursoId)
    {
        var curso = await _context.Cursos.FindAsync(cursoId);
        var alumnos = await _context.Alumnos
            .Where(a => a.CursoId == cursoId && a.Estado == EstadoAlumno.Activo)
            .ToListAsync();

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Alumnos");

        // Encabezados
        sheet.Cell(1, 1).Value = "ID";
        sheet.Cell(1, 2).Value = "Apellido";
        sheet.Cell(1, 3).Value = "Nombre";
        sheet.Cell(1, 4).Value = "DNI";
        sheet.Cell(1, 5).Value = "Curso";

        // Estilo encabezados
        var headerRow = sheet.Range("A1:E1");
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightBlue;

        // Datos
        for (int i = 0; i < alumnos.Count; i++)
        {
            var a = alumnos[i];
            sheet.Cell(i + 2, 1).Value = a.Id;
            sheet.Cell(i + 2, 2).Value = a.Apellido;
            sheet.Cell(i + 2, 3).Value = a.Nombre;
            sheet.Cell(i + 2, 4).Value = a.DNI;
            sheet.Cell(i + 2, 5).Value = curso != null ? $"{curso.Nombre} {curso.Anio}/{curso.Division}" : "";
        }

        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<List<string>> ImportarAlumnosAsync(byte[] archivo)
    {
        var mensajes = new List<string>();

        using var stream = new MemoryStream(archivo);
        using var workbook = new XLWorkbook(stream);
        var sheet = workbook.Worksheet(1);

        var filas = sheet.RowsUsed().Skip(1); // saltear encabezados

        foreach (var fila in filas)
        {
            var nombre = fila.Cell(1).GetString().Trim();
            var apellido = fila.Cell(2).GetString().Trim();
            var dni = fila.Cell(3).GetString().Trim();
            var cursoIdStr = fila.Cell(4).GetString().Trim();

            if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(apellido) || string.IsNullOrEmpty(dni))
            {
                mensajes.Add($"Fila {fila.RowNumber()}: faltan datos obligatorios (Nombre, Apellido o DNI). Saltada.");
                continue;
            }

            // Verificar DNI duplicado
            var existe = await _context.Alumnos.AnyAsync(a => a.DNI == dni);
            if (existe)
            {
                mensajes.Add($"Fila {fila.RowNumber()}: DNI {dni} ya existe en el sistema. Saltado.");
                continue;
            }

            // Verificar CursoId
            int? cursoId = null;
            if (!string.IsNullOrEmpty(cursoIdStr) && int.TryParse(cursoIdStr, out int parsedCursoId))
            {
                var cursoExiste = await _context.Cursos.AnyAsync(c => c.Id == parsedCursoId);
                if (cursoExiste)
                    cursoId = parsedCursoId;
                else
                    mensajes.Add($"Fila {fila.RowNumber()}: CursoId {parsedCursoId} no existe. El alumno se importará sin curso.");
            }

            var alumno = new Models.Alumno
            {
                Nombre = nombre,
                Apellido = apellido,
                DNI = dni,
                CursoId = cursoId,
                Estado = Enums.EstadoAlumno.Activo,
                FechaBaja = null
            };

            _context.Alumnos.Add(alumno);
            await _context.SaveChangesAsync();

            mensajes.Add($"Fila {fila.RowNumber()}: alumno {apellido}, {nombre} importado correctamente.");
        }

        return mensajes;
    }
}