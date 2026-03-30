using Microsoft.Extensions.Configuration;
using GestionApiClient.DTOs;
using GestionApiClient.Clients;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var baseUrl = config["ApiBaseUrl"]!;
var client = new GestionApiClient.GestionApiClient(baseUrl);

bool salir = false;

while (!salir)
{
    Console.Clear();
    Console.WriteLine("=== GESTIÓN ACADÉMICA ===\n");
    Console.WriteLine("1. Cursos");
    Console.WriteLine("2. Alumnos");
    Console.WriteLine("3. Asistencias");
    Console.WriteLine("4. Importar / Exportar");
    Console.WriteLine("0. Salir\n");
    Console.Write("Elegí una opción: ");

    var opcion = Console.ReadLine();

    switch (opcion)
    {
        case "1":
            await MenuCursos();
            break;
        case "2":
            await MenuAlumnos();
            break;
        case "3":
            await MenuAsistencias();
            break;
        case "4":
            await MenuEtl();
            break;
        case "0":
            salir = true;
            break;
        default:
            Console.WriteLine("\nOpción inválida. Presioná cualquier tecla para volver.");
            Console.ReadKey();
            break;
    }
}

//
//Esta es la seccion con toda la logica para los cursos. Cada opcion del menu de cursos llama a una funcion que se encarga de realizar la accion correspondiente (ver, crear, editar, eliminar).
//
#region Cursos
async Task MenuCursos()
{
    bool volver = false;

    while (!volver)
    {
        Console.Clear();
        Console.WriteLine("=== CURSOS ===\n");
        Console.WriteLine("1. Ver todos los cursos");
        Console.WriteLine("2. Crear curso");
        Console.WriteLine("3. Editar curso");
        Console.WriteLine("4. Eliminar curso");
        Console.WriteLine("0. Volver\n");
        Console.Write("Elegí una opción: ");

        var opcion = Console.ReadLine();

        switch (opcion)
        {
            case "1":
                await VerCursos();
                break;
            case "2":
                await CrearCurso();
                break;
            case "3":
                await EditarCurso();
                break;
            case "4":
                await EliminarCurso();
                break;
            case "0":
                volver = true;
                break;
            default:
                Console.WriteLine("\nOpción inválida. Presioná cualquier tecla para volver.");
                Console.ReadKey();
                break;
        }
    }
}


async Task VerCursos()
{
    Console.Clear();
    Console.WriteLine("=== VER CURSOS ===\n");

    var cursos = await client.Cursos.GetAllAsync();

    if (cursos.Count == 0)
    {
        Console.WriteLine("No hay cursos registrados.");
    }
    else
    {
        foreach (var curso in cursos)
        {
            Console.WriteLine($"[{curso.Id}] {curso.Nombre} - {curso.Anio} / {curso.Division}");
        }
    }

    Console.WriteLine("\nPresioná cualquier tecla para volver.");
    Console.ReadKey();
}

async Task CrearCurso()
{
    Console.Clear();
    Console.WriteLine("=== CREAR CURSO ===\n");

    Console.Write("Nombre: ");
    var nombre = Console.ReadLine()!;

    Console.Write("Año: ");
    var anio = int.Parse(Console.ReadLine()!);

    Console.Write("División: ");
    var division = Console.ReadLine()!;

    var dto = new GestionApiClient.DTOs.CursoDTO
    {
        Nombre = nombre,
        Anio = anio,
        Division = division
    };

    var creado = await client.Cursos.CreateAsync(dto);

    if (creado != null)
        Console.WriteLine($"\nCurso creado correctamente con ID {creado.Id}.");
    else
        Console.WriteLine("\nError al crear el curso.");

    Console.WriteLine("Presioná cualquier tecla para volver.");
    Console.ReadKey();
}

async Task EditarCurso()
{
    Console.Clear();
    Console.WriteLine("=== EDITAR CURSO ===\n");

    var cursos = await client.Cursos.GetAllAsync();
    foreach (var c in cursos)
        Console.WriteLine($"[{c.Id}] {c.Nombre} - {c.Anio} / {c.Division}");

    Console.Write("\nIngresá el ID del curso a editar: ");
    var id = int.Parse(Console.ReadLine()!);

    Console.Write("Nuevo nombre: ");
    var nombre = Console.ReadLine()!;

    Console.Write("Nuevo año: ");
    var anio = int.Parse(Console.ReadLine()!);

    Console.Write("Nueva división: ");
    var division = Console.ReadLine()!;

    var dto = new GestionApiClient.DTOs.CursoDTO
    {
        Nombre = nombre,
        Anio = anio,
        Division = division
    };

    var actualizado = await client.Cursos.UpdateAsync(id, dto);

    if (actualizado != null)
        Console.WriteLine("\nCurso actualizado correctamente.");
    else
        Console.WriteLine("\nNo se encontró el curso.");

    Console.WriteLine("Presioná cualquier tecla para volver.");
    Console.ReadKey();
}

async Task EliminarCurso()
{
    Console.Clear();
    Console.WriteLine("=== ELIMINAR CURSO ===\n");

    var cursos = await client.Cursos.GetAllAsync();
    foreach (var c in cursos)
        Console.WriteLine($"[{c.Id}] {c.Nombre} - {c.Anio} / {c.Division}");

    Console.Write("\nIngresá el ID del curso a eliminar: ");
    var id = int.Parse(Console.ReadLine()!);

    var resultado = await client.Cursos.DeleteAsync(id);

    if (resultado)
        Console.WriteLine("\nCurso eliminado correctamente.");
    else
        Console.WriteLine("\nNo se pudo eliminar. El curso no existe o tiene alumnos activos.");

    Console.WriteLine("Presioná cualquier tecla para volver.");
    Console.ReadKey();
}
#endregion


//
//Esta es la seccion de alumnos que se encarga de toda la logica relacionada a los alumnos. Al igual que con los cursos, cada opcion del menu de alumnos llama a una funcion que se encarga de realizar la accion correspondiente (ver, crear, editar, eliminar).
//
#region Alumnos

async Task MenuAlumnos()
{
    bool volver = false;

    while (!volver)
    {
        Console.Clear();
        Console.WriteLine("=== ALUMNOS ===\n");
        Console.WriteLine("1. Ver alumnos de un curso");
        Console.WriteLine("2. Ver ex-alumnos");
        Console.WriteLine("3. Ver egresados");
        Console.WriteLine("4. Crear alumno");
        Console.WriteLine("5. Editar alumno");
        Console.WriteLine("6. Dar de baja alumno");
        Console.WriteLine("7. Marcar como egresado");
        Console.WriteLine("8. Ver historial de faltas");
        Console.WriteLine("9. Mover alumno a otro curso");
        Console.WriteLine("10. Ver todos los alumnos");
        Console.WriteLine("0. Volver\n");
        Console.Write("Elegí una opción: ");

        var opcion = Console.ReadLine();

        switch (opcion)
        {
            case "1":
                await VerAlumnosDeCurso();
                break;
            case "2":
                await VerExAlumnos();
                break;
            case "3":
                await VerEgresados();
                break;
            case "4":
                await AgregarAlumno();
                break;
            case "5":
                await EditarAlumno();
                break;
            case "6":
                await DarDeBajaAlumno();
                break;
            case "7":
                await MarcarEgresado();
                break;
            case "8":
                await VerHistorialAlumno();
                break;
            case "9":
                await MoverAlumno();
                break;
            case "10":
                await VerTodosLosAlumnos();
                break;
            case "0":
                volver = true;
                break;
            default:
                Console.WriteLine("\nOpción inválida. Presioná cualquier tecla para volver.");
                Console.ReadKey();
                break;
        }
    }
}

async Task VerAlumnosDeCurso()
{
    Console.Clear();
    Console.WriteLine("=== ALUMNOS DE UN CURSO ===\n");

    var cursos = await client.Cursos.GetAllAsync();
    foreach (var c in cursos)
        Console.WriteLine($"[{c.Id}] {c.Nombre} - {c.Anio} / {c.Division}");

    Console.Write("\nIngresá el ID del curso: ");
    var cursoId = int.Parse(Console.ReadLine()!);

    var alumnos = await client.Alumnos.GetActivosByCursoAsync(cursoId);

    Console.WriteLine();
    if (alumnos.Count == 0)
        Console.WriteLine("No hay alumnos activos en este curso.");
    else
        foreach (var a in alumnos)
            Console.WriteLine($"[{a.Id}] {a.Apellido}, {a.Nombre} - DNI: {a.DNI}");

    Console.WriteLine("\nPresioná cualquier tecla para volver.");
    Console.ReadKey();
}

async Task VerExAlumnos()
{
    Console.Clear();
    Console.WriteLine("=== EX-ALUMNOS ===\n");

    var alumnos = await client.Alumnos.GetExAlumnosAsync();

    if (alumnos.Count == 0)
        Console.WriteLine("No hay ex-alumnos registrados.");
    else
        foreach (var a in alumnos)
            Console.WriteLine($"[{a.Id}] {a.Apellido}, {a.Nombre} - DNI: {a.DNI} - Baja: {a.FechaBaja?.ToString("dd/MM/yyyy")}");

    Console.WriteLine("\nPresioná cualquier tecla para volver.");
    Console.ReadKey();
}

async Task VerEgresados()
{
    Console.Clear();
    Console.WriteLine("=== EGRESADOS ===\n");

    var alumnos = await client.Alumnos.GetEgresadosAsync();

    if (alumnos.Count == 0)
        Console.WriteLine("No hay egresados registrados.");
    else
        foreach (var a in alumnos)
            Console.WriteLine($"[{a.Id}] {a.Apellido}, {a.Nombre} - DNI: {a.DNI} - Egreso: {a.FechaBaja?.ToString("dd/MM/yyyy")}");

    Console.WriteLine("\nPresioná cualquier tecla para volver.");
    Console.ReadKey();
}

async Task AgregarAlumno()
{
    Console.Clear();
    Console.WriteLine("=== AGREGAR ALUMNO ===\n");

    Console.Write("Nombre: ");
    var nombre = Console.ReadLine()!;

    Console.Write("Apellido: ");
    var apellido = Console.ReadLine()!;

    Console.Write("DNI: ");
    var dni = Console.ReadLine()!;

    var dto = new AlumnoDTO
    {
        Nombre = nombre,
        Apellido = apellido,
        DNI = dni,
        CursoId = null
    };

    var creado = await client.Alumnos.CreateAsync(dto);

    if (creado != null)
        Console.WriteLine($"\nAlumno agregado correctamente con ID {creado.Id}.");
    else
        Console.WriteLine("\nError al agregar el alumno.");

    Console.WriteLine("Presioná cualquier tecla para volver.");
    Console.ReadKey();
}

async Task EditarAlumno()
{
    Console.Clear();
    Console.WriteLine("=== EDITAR ALUMNO ===\n");

    Console.Write("Ingresá el ID del alumno a editar: ");
    var id = int.Parse(Console.ReadLine()!);

    var alumno = await client.Alumnos.GetByIdAsync(id);
    if (alumno == null)
    {
        Console.WriteLine("\nNo se encontró el alumno.");
        Console.ReadKey();
        return;
    }

    Console.WriteLine($"\nAlumno actual: {alumno.Apellido}, {alumno.Nombre} - DNI: {alumno.DNI}");

    Console.Write("Nuevo nombre: ");
    var nombre = Console.ReadLine()!;

    Console.Write("Nuevo apellido: ");
    var apellido = Console.ReadLine()!;

    Console.Write("Nuevo DNI: ");
    var dni = Console.ReadLine()!;

    var dto = new GestionApiClient.DTOs.AlumnoDTO
    {
        Nombre = nombre,
        Apellido = apellido,
        DNI = dni
    };

    var actualizado = await client.Alumnos.UpdateAsync(id, dto);

    if (actualizado != null)
        Console.WriteLine("\nAlumno actualizado correctamente.");
    else
        Console.WriteLine("\nError al actualizar el alumno.");

    Console.WriteLine("Presioná cualquier tecla para volver.");
    Console.ReadKey();
}

async Task DarDeBajaAlumno()
{
    Console.Clear();
    Console.WriteLine("=== DAR DE BAJA ALUMNO ===\n");

    Console.Write("Ingresá el ID del alumno: ");
    var id = int.Parse(Console.ReadLine()!);

    var resultado = await client.Alumnos.BajaAsync(id);

    if (resultado)
        Console.WriteLine("\nAlumno dado de baja correctamente.");
    else
        Console.WriteLine("\nNo se encontró el alumno.");

    Console.WriteLine("Presioná cualquier tecla para volver.");
    Console.ReadKey();
}

async Task MarcarEgresado()
{
    Console.Clear();
    Console.WriteLine("=== MARCAR COMO EGRESADO ===\n");

    Console.Write("Ingresá el ID del alumno: ");
    var id = int.Parse(Console.ReadLine()!);

    var resultado = await client.Alumnos.EgresarAsync(id);

    if (resultado)
        Console.WriteLine("\nAlumno marcado como egresado correctamente.");
    else
        Console.WriteLine("\nNo se encontró el alumno.");

    Console.WriteLine("Presioná cualquier tecla para volver.");
    Console.ReadKey();
}

async Task VerHistorialAlumno()
{
    Console.Clear();
    Console.WriteLine("=== HISTORIAL DE FALTAS ===\n");

    Console.Write("Ingresá el ID del alumno: ");
    var id = int.Parse(Console.ReadLine()!);

    var resumen = await client.Asistencias.GetResumenAlumnoAsync(id);

    Console.WriteLine();
    foreach (var item in resumen)
        Console.WriteLine($"{item.Key}: {item.Value}");

    Console.WriteLine("\nPresioná cualquier tecla para volver.");
    Console.ReadKey();
}

async Task MoverAlumno()
{
    Console.Clear();
    Console.WriteLine("=== MOVER ALUMNO A OTRO CURSO ===\n");

    Console.Write("Ingresá el ID del alumno: ");
    var alumnoId = int.Parse(Console.ReadLine()!);

    var cursos = await client.Cursos.GetAllAsync();
    foreach (var c in cursos)
        Console.WriteLine($"[{c.Id}] {c.Nombre} - {c.Anio} / {c.Division}");

    Console.Write("\nIngresá el ID del curso destino: ");
    var cursoId = int.Parse(Console.ReadLine()!);

    var resultado = await client.Alumnos.MoverCursoAsync(alumnoId, cursoId);

    if (resultado)
        Console.WriteLine("\nAlumno movido correctamente.");
    else
        Console.WriteLine("\nNo se encontró el alumno o el curso.");

    Console.WriteLine("Presioná cualquier tecla para volver.");
    Console.ReadKey();
}

async Task VerTodosLosAlumnos()
{
    Console.Clear();
    Console.WriteLine("=== TODOS LOS ALUMNOS ===\n");

    int pagina = 1;
    bool salir = false;

    while (!salir)
    {
        var alumnos = await client.Alumnos.GetTodosAsync(pagina);

        Console.Clear();
        Console.WriteLine($"=== TODOS LOS ALUMNOS === Página {pagina}\n");

        if (alumnos.Count == 0)
        {
            Console.WriteLine("No hay más alumnos.");
        }
        else
        {
            foreach (var a in alumnos)
                Console.WriteLine($"[{a.Id}] {a.Apellido}, {a.Nombre} - DNI: {a.DNI} - CursoId: {a.CursoId?.ToString() ?? "Sin curso"}");
        }

        Console.WriteLine("\nN = siguiente página | A = página anterior | 0 = volver");
        var opcion = Console.ReadLine()?.ToUpper();

        switch (opcion)
        {
            case "N":
                if (alumnos.Count == 10) pagina++;
                else Console.WriteLine("No hay más páginas.");
                break;
            case "A":
                if (pagina > 1) pagina--;
                break;
            case "0":
                salir = true;
                break;
        }
    }
}

#endregion

//seccion de asistencias, similar a las anteriores pero con opciones para marcar asistencia, justificar falta, etc.

#region Asistencias

async Task MenuAsistencias()
{
    bool volver = false;

    while (!volver)
    {
        Console.Clear();
        Console.WriteLine("=== ASISTENCIAS ===\n");
        Console.WriteLine("1. Registrar asistencia del día");
        Console.WriteLine("2. Editar asistencia");
        Console.WriteLine("3. Ver asistencias de un curso hoy");
        Console.WriteLine("4. Ver resumen de un alumno");
        Console.WriteLine("0. Volver\n");
        Console.Write("Elegí una opción: ");

        var opcion = Console.ReadLine();

        switch (opcion)
        {
            case "1":
                await RegistrarAsistencia();
                break;
            case "2":
                await EditarAsistencia();
                break;
            case "3":
                await VerAsistenciasHoy();
                break;
            case "4":
                await VerResumenAlumno();
                break;
            case "0":
                volver = true;
                break;
            default:
                Console.WriteLine("\nOpción inválida. Presioná cualquier tecla para volver.");
                Console.ReadKey();
                break;
        }
    }
}

async Task RegistrarAsistencia()
{
    Console.Clear();
    Console.WriteLine("=== REGISTRAR ASISTENCIA DEL DÍA ===\n");

    var cursos = await client.Cursos.GetAllAsync();
    foreach (var c in cursos)
        Console.WriteLine($"[{c.Id}] {c.Nombre} - {c.Anio} / {c.Division}");

    Console.Write("\nIngresá el ID del curso: ");
    var cursoId = int.Parse(Console.ReadLine()!);

    var alumnos = await client.Alumnos.GetActivosByCursoAsync(cursoId);

    if (alumnos.Count == 0)
    {
        Console.WriteLine("\nNo hay alumnos activos en este curso.");
        Console.ReadKey();
        return;
    }

    var asistencias = new List<AsistenciaDTO>();

    Console.WriteLine("\nEstados: 0=Presente | 1=Ausente | 2=Tarde | 3=AusenteConPresencia | 4=AusenteJustificado\n");

    foreach (var alumno in alumnos)
    {
        Console.Write($"{alumno.Apellido}, {alumno.Nombre}: ");
        var estado = int.Parse(Console.ReadLine()!);

        asistencias.Add(new AsistenciaDTO
        {
            AlumnoId = alumno.Id,
            CursoId = cursoId,
            Fecha = DateOnly.FromDateTime(DateTime.Today),
            Estado = estado
        });
    }

    var resultado = await client.Asistencias.BulkCreateAsync(asistencias);

    if (resultado)
        Console.WriteLine("\nAsistencias registradas correctamente.");
    else
        Console.WriteLine("\nError al registrar las asistencias.");

    Console.WriteLine("Presioná cualquier tecla para volver.");
    Console.ReadKey();
}

async Task EditarAsistencia()
{
    Console.Clear();
    Console.WriteLine("=== EDITAR ASISTENCIA ===\n");

    var cursos = await client.Cursos.GetAllAsync();
    foreach (var c in cursos)
        Console.WriteLine($"[{c.Id}] {c.Nombre} - {c.Anio} / {c.Division}");

    Console.Write("\nIngresá el ID del curso: ");
    var cursoId = int.Parse(Console.ReadLine()!);

    var asistencias = await client.Asistencias.GetByCursoHoyAsync(cursoId);

    if (asistencias.Count == 0)
    {
        Console.WriteLine("\nNo hay asistencias registradas hoy para este curso.");
        Console.ReadKey();
        return;
    }

    foreach (var a in asistencias)
        Console.WriteLine($"[{a.Id}] AlumnoID: {a.AlumnoId} - Estado: {a.Estado}");

    Console.Write("\nIngresá el ID de la asistencia a editar: ");
    var id = int.Parse(Console.ReadLine()!);

    Console.WriteLine("\nEstados: 0=Presente | 1=Ausente | 2=Tarde | 3=AusenteConPresencia | 4=AusenteJustificado");
    Console.Write("Nuevo estado: ");
    var estado = int.Parse(Console.ReadLine()!);

    var dto = new AsistenciaDTO
    {
        Estado = estado
    };

    var actualizado = await client.Asistencias.UpdateAsync(id, dto);

    if (actualizado != null)
        Console.WriteLine("\nAsistencia actualizada correctamente.");
    else
        Console.WriteLine("\nNo se encontró la asistencia.");

    Console.WriteLine("Presioná cualquier tecla para volver.");
    Console.ReadKey();
}

async Task VerAsistenciasHoy()
{
    Console.Clear();
    Console.WriteLine("=== ASISTENCIAS DE HOY ===\n");

    var cursos = await client.Cursos.GetAllAsync();
    foreach (var c in cursos)
        Console.WriteLine($"[{c.Id}] {c.Nombre} - {c.Anio} / {c.Division}");

    Console.Write("\nIngresá el ID del curso: ");
    var cursoId = int.Parse(Console.ReadLine()!);

    var asistencias = await client.Asistencias.GetByCursoHoyAsync(cursoId);

    Console.WriteLine();
    if (asistencias.Count == 0)
        Console.WriteLine("No hay asistencias registradas hoy para este curso.");
    else
        foreach (var a in asistencias)
            Console.WriteLine($"AlumnoID: {a.AlumnoId} - Estado: {a.Estado} - Fecha: {a.Fecha}");

    Console.WriteLine("\nPresioná cualquier tecla para volver.");
    Console.ReadKey();
}

async Task VerResumenAlumno()
{
    Console.Clear();
    Console.WriteLine("=== RESUMEN DE ASISTENCIAS ===\n");

    Console.Write("Ingresá el ID del alumno: ");
    var id = int.Parse(Console.ReadLine()!);

    var resumen = await client.Asistencias.GetResumenAlumnoAsync(id);

    Console.WriteLine();
    foreach (var item in resumen)
        Console.WriteLine($"{item.Key}: {item.Value}");

    Console.WriteLine("\nPresioná cualquier tecla para volver.");
    Console.ReadKey();
}

#endregion

//este es el menu con etl para importar y exportar datos. Por ahora solo tiene la opcion de exportar alumnos de un curso a excel, pero se pueden agregar mas opciones si se desea.
#region ETL

async Task MenuEtl()
{
    bool volver = false;

    while (!volver)
    {
        Console.Clear();
        Console.WriteLine("=== IMPORTAR / EXPORTAR ===\n");
        Console.WriteLine("1. Exportar alumnos de un curso a Excel");
        Console.WriteLine("2. Importar alumnos desde Excel");
        Console.WriteLine("0. Volver\n");
        Console.Write("Elegí una opción: ");

        var opcion = Console.ReadLine();

        switch (opcion)
        {
            case "1":
                await ExportarAlumnos();
                break;
            case "2":
                await ImportarAlumnos();
                break;
            case "0":
                volver = true;
                break;
            default:
                Console.WriteLine("\nOpción inválida. Presioná cualquier tecla para volver.");
                Console.ReadKey();
                break;
        }
    }
}

async Task ExportarAlumnos()
{
    Console.Clear();
    Console.WriteLine("=== EXPORTAR ALUMNOS A EXCEL ===\n");

    var cursos = await client.Cursos.GetAllAsync();
    foreach (var c in cursos)
        Console.WriteLine($"[{c.Id}] {c.Nombre} - {c.Anio} / {c.Division}");

    Console.Write("\nIngresá el ID del curso: ");
    var cursoId = int.Parse(Console.ReadLine()!);

    var archivo = await client.Etl.ExportarAlumnosAsync(cursoId);

    if (archivo == null)
    {
        Console.WriteLine("\nError al exportar.");
        Console.ReadKey();
        return;
    }

    Console.Write("\nIngresá la ruta donde guardar el archivo (ej: C:\\Users\\Usuario\\Desktop\\alumnos.xlsx): ");
    var ruta = Console.ReadLine()!;

    await File.WriteAllBytesAsync(ruta, archivo);
    Console.WriteLine("\nArchivo exportado correctamente.");

    Console.WriteLine("Presioná cualquier tecla para volver.");
    Console.ReadKey();
}

async Task ImportarAlumnos()
{
    Console.Clear();
    Console.WriteLine("=== IMPORTAR ALUMNOS DESDE EXCEL ===\n");

    Console.Write("Ingresá la ruta del archivo Excel: ");
    var ruta = Console.ReadLine()!;

    if (!File.Exists(ruta))
    {
        Console.WriteLine("\nNo se encontró el archivo.");
        Console.ReadKey();
        return;
    }

    var archivo = await File.ReadAllBytesAsync(ruta);
    var mensajes = await client.Etl.ImportarAlumnosAsync(archivo);

    if (mensajes == null)
    {
        Console.WriteLine("\nError al importar.");
        Console.ReadKey();
        return;
    }

    Console.WriteLine("\n=== RESULTADO DE LA IMPORTACIÓN ===\n");
    foreach (var mensaje in mensajes)
        Console.WriteLine(mensaje);

    Console.WriteLine("\nPresioná cualquier tecla para volver.");
    Console.ReadKey();
}

#endregion