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

        XLWorkbook workbook;

        try
        {
            using var stream = new MemoryStream(archivo);
            workbook = new XLWorkbook(stream);
        }
        catch (Exception)
        {
            mensajes.Add("El archivo no es un Excel válido o está corrupto.");
            return mensajes;
        }

        var sheet = workbook.Worksheet(1);
        var filas = sheet.RowsUsed().Skip(1);

        foreach (var fila in filas)
        {
            try
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

                var existe = await _context.Alumnos.AnyAsync(a => a.DNI == dni);
                if (existe)
                {
                    mensajes.Add($"Fila {fila.RowNumber()}: DNI {dni} ya existe en el sistema. Saltado.");
                    continue;
                }

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
            catch (Exception ex)
            {
                mensajes.Add($"Fila {fila.RowNumber()}: error inesperado — {ex.Message}. Saltada.");
            }
        }

        workbook.Dispose();
        return mensajes;
    }

    public async Task<byte[]> ExportarAsistenciasMesAsync(int cursoId, int anio, int mes)
    {
        var curso = await _context.Cursos.FindAsync(cursoId);
        var alumnos = await _context.Alumnos
            .Where(a => a.CursoId == cursoId && a.Estado == EstadoAlumno.Activo)
            .OrderBy(a => a.Apellido)
            .ToListAsync();

        var diasDelMes = DateTime.DaysInMonth(anio, mes);
        var fechaInicio = new DateOnly(anio, mes, 1);
        var fechaFin = new DateOnly(anio, mes, diasDelMes);

        var asistencias = await _context.Asistencias
            .Where(a => a.CursoId == cursoId && a.Fecha >= fechaInicio && a.Fecha <= fechaFin)
            .ToListAsync();

        var abreviaturas = new Dictionary<EstadoAsistencia, string>
    {
        { EstadoAsistencia.Presente,            "P"  },
        { EstadoAsistencia.Ausente,             "A"  },
        { EstadoAsistencia.Tarde,               "T"  },
        { EstadoAsistencia.AusenteConPresencia, "AP" },
        { EstadoAsistencia.AusenteJustificado,  "AJ" }
    };

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Asistencias");

        // Encabezado del curso
        sheet.Cell(1, 1).Value = $"{curso?.Nombre} — {anio}/{mes:D2}";
        sheet.Range(1, 1, 1, diasDelMes + 1).Merge();
        sheet.Cell(1, 1).Style.Font.Bold = true;
        sheet.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.LightBlue;

        // Encabezados de columnas
        sheet.Cell(2, 1).Value = "Alumno";
        sheet.Cell(2, 1).Style.Font.Bold = true;

        for (int dia = 1; dia <= diasDelMes; dia++)
        {
            var fecha = new DateOnly(anio, mes, dia);
            var celda = sheet.Cell(2, dia + 1);
            celda.Value = fecha.ToString("dd/MM");
            celda.Style.Font.Bold = true;
            celda.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Fin de semana en gris
            if (fecha.DayOfWeek == DayOfWeek.Saturday || fecha.DayOfWeek == DayOfWeek.Sunday)
                celda.Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        // Filas de alumnos
        for (int i = 0; i < alumnos.Count; i++)
        {
            var alumno = alumnos[i];
            var fila = i + 3;

            sheet.Cell(fila, 1).Value = $"{alumno.Apellido}, {alumno.Nombre}";

            for (int dia = 1; dia <= diasDelMes; dia++)
            {
                var fecha = new DateOnly(anio, mes, dia);
                var asistencia = asistencias
                    .FirstOrDefault(a => a.AlumnoId == alumno.Id && a.Fecha == fecha);

                var celda = sheet.Cell(fila, dia + 1);
                celda.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                if (fecha.DayOfWeek == DayOfWeek.Saturday || fecha.DayOfWeek == DayOfWeek.Sunday)
                {
                    celda.Style.Fill.BackgroundColor = XLColor.LightGray;
                    continue;
                }

                if (asistencia != null)
                {
                    celda.Value = abreviaturas[asistencia.Estado];

                    celda.Style.Fill.BackgroundColor = asistencia.Estado switch
                    {
                        EstadoAsistencia.Presente => XLColor.LightGreen,
                        EstadoAsistencia.Ausente => XLColor.LightCoral,
                        EstadoAsistencia.Tarde => XLColor.LightYellow,
                        EstadoAsistencia.AusenteConPresencia => XLColor.LightBlue,
                        EstadoAsistencia.AusenteJustificado => XLColor.LightSalmon,
                        _ => XLColor.White
                    };
                }
            }
        }

        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}