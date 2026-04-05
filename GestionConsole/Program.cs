using Microsoft.Extensions.Configuration;
using GestionApiClient.DTOs;
using GestionApiClient.Clients;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var baseUrl = config["ApiBaseUrl"]!;
var client = new GestionApiClient.GestionApiClient(baseUrl);

//Este string se encarga para luego almacenar el rol de usuario
string? rolActual = null;

// Helpers de permisos
bool EsAdminODirectivo() => rolActual == "Admin" || rolActual == "Directivo";

while (rolActual == null)
{
    Console.Clear();
    Console.WriteLine("=== GESTIÓN ACADÉMICA ===\n");
    Console.WriteLine("1. Iniciar sesión");
    Console.WriteLine("2. Activar cuenta");
    Console.WriteLine("0. Salir\n");
    Console.Write("Elegí una opción: ");

    var opcionInicio = Console.ReadLine();

    switch (opcionInicio)
    {
        case "1":
            rolActual = await Login();
            break;
        case "2":
            await ActivarCuenta();
            break;
        case "0":
            return;
        default:
            Console.WriteLine("\nOpción inválida. Presioná cualquier tecla para volver.");
            Console.ReadKey();
            break;
    }
}


bool salir = false;

while (!salir)
{
    Console.Clear();
    Console.WriteLine($"=== GESTIÓN ACADÉMICA === [{rolActual}]\n");
    Console.WriteLine("1. Cursos");
    Console.WriteLine("2. Alumnos");
    Console.WriteLine("3. Asistencias");

    if (rolActual == "Admin" || rolActual == "Directivo")
        Console.WriteLine("4. Importar / Exportar");

    if (rolActual == "Admin")
        Console.WriteLine("5. Usuarios");

    Console.WriteLine("6. Cambiar mi contraseña");

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
            if (EsAdminODirectivo())
                await MenuEtl();
            else
                Console.WriteLine("\nNo tenés permisos. Presioná cualquier tecla.");
            Console.ReadKey();
            break;
        case "5":
            if (rolActual == "Admin")
                await MenuUsuarios();
            else
                Console.WriteLine("\nNo tenés permisos. Presioná cualquier tecla.");
            Console.ReadKey();
            break;
        case "6":
            await CambiarMiPassword();
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

#region Auth

async Task<string?> Login()
{
    Console.Clear();
    Console.WriteLine("=== INICIAR SESIÓN ===\n");

    Console.Write("Usuario: ");
    var username = Console.ReadLine()!;

    Console.Write("Contraseña: ");
    var password = Console.ReadLine()!;

    var token = await client.Auth.LoginAsync(username, password);

    if (token == null)
    {
        Console.WriteLine("\nCredenciales inválidas. Presioná cualquier tecla para volver.");
        Console.ReadKey();
        return null;
    }

    client.SetToken(token);

    // Decodificar el rol del token
    var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
    var jwtToken = handler.ReadJwtToken(token);
    var rol = jwtToken.Claims
        .FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
        ?.Value;

    Console.WriteLine($"\nBienvenido. Rol: {rol}");
    Console.ReadKey();
    return rol;
}

async Task ActivarCuenta()
{
    Console.Clear();
    Console.WriteLine("=== ACTIVAR CUENTA ===\n");

    Console.Write("Token de activación: ");
    var token = Console.ReadLine()!;

    Console.Write("Nombre de usuario: ");
    var username = Console.ReadLine()!;

    Console.Write("Contraseña: ");
    var password = Console.ReadLine()!;

    var resultado = await client.Auth.ActivarCuentaAsync(token, username, password);

    if (resultado)
        Console.WriteLine("\nCuenta activada correctamente. Ya podés iniciar sesión.");
    else
        Console.WriteLine("\nToken inválido o nombre de usuario ya existe.");

    Console.WriteLine("Presioná cualquier tecla para volver.");
    Console.ReadKey();
}

#endregion


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

        if (EsAdminODirectivo())
        {
            Console.WriteLine("2. Crear curso");
            Console.WriteLine("3. Editar curso");
            Console.WriteLine("4. Eliminar curso");
        }

        Console.WriteLine("0. Volver\n");
        Console.Write("Elegí una opción: ");

        var opcion = Console.ReadLine();
        switch (opcion)
        {
            case "1": await VerCursos(); break;
            case "2": if (EsAdminODirectivo()) await CrearCurso(); break;
            case "3": if (EsAdminODirectivo()) await EditarCurso(); break;
            case "4": if (EsAdminODirectivo()) await EliminarCurso(); break;
            case "0": volver = true; break;
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
    if (!int.TryParse(Console.ReadLine(), out int id))
    {
        Console.WriteLine("\nID inválido.");
        Console.ReadKey();
        return;
    }

    var curso = await client.Cursos.GetByIdAsync(id);
    if (curso == null)
    {
        Console.WriteLine("\nNo se encontró el curso.");
        Console.ReadKey();
        return;
    }

    Console.WriteLine($"\nCurso actual: {curso.Nombre} - {curso.Anio} / {curso.Division}");

    Console.Write("Nuevo nombre: ");
    var nombre = Console.ReadLine()!;

    Console.Write("Nuevo año: ");
    if (!int.TryParse(Console.ReadLine(), out int anio))
    {
        Console.WriteLine("\nAño inválido, debe ser un número.");
        Console.ReadKey();
        return;
    }

    Console.Write("Nueva división: ");
    var division = Console.ReadLine()!;

    var dto = new CursoDTO
    {
        Nombre = nombre,
        Anio = anio,
        Division = division
    };

    var actualizado = await client.Cursos.UpdateAsync(id, dto);

    if (actualizado != null)
        Console.WriteLine("\nCurso actualizado correctamente.");
    else
        Console.WriteLine("\nError al actualizar el curso.");

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
    if (!int.TryParse(Console.ReadLine(), out int id))
    {
        Console.WriteLine("\nID inválido.");
        Console.ReadKey();
        return;
    }

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
        Console.WriteLine("4. Ver todos los alumnos");
        Console.WriteLine("5. Ver historial de faltas");

        if (EsAdminODirectivo())
        {
            Console.WriteLine("6. Crear alumno");
            Console.WriteLine("7. Editar alumno");
            Console.WriteLine("8. Mover alumno a otro curso");
            Console.WriteLine("9. Dar de baja alumno");
            Console.WriteLine("10. Marcar como egresado");
        }

        Console.WriteLine("0. Volver\n");
        Console.Write("Elegí una opción: ");

        var opcion = Console.ReadLine();
        switch (opcion)
        {
            case "1": await VerAlumnosDeCurso(); break;
            case "2": await VerExAlumnos(); break;
            case "3": await VerEgresados(); break;
            case "4": await VerTodosLosAlumnos(); break;
            case "5": await VerHistorialAlumno(); break;
            case "6": if (EsAdminODirectivo()) await AgregarAlumno(); break;
            case "7": if (EsAdminODirectivo()) await EditarAlumno(); break;
            case "8": if (EsAdminODirectivo()) await MoverAlumno(); break;
            case "9": if (EsAdminODirectivo()) await DarDeBajaAlumno(); break;
            case "10": if (EsAdminODirectivo()) await MarcarEgresado(); break;
            case "0": volver = true; break;
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
    if (!int.TryParse(Console.ReadLine(), out int cursoId))
    {
        Console.WriteLine("\nID inválido.");
        Console.ReadKey();
        return;
    }

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
    var nombre = Console.ReadLine()!.Trim();
    if (string.IsNullOrWhiteSpace(nombre) || !nombre.All(c => char.IsLetter(c) || c == ' '))
    {
        Console.WriteLine("\nEl nombre solo puede contener letras.");
        Console.ReadKey();
        return;
    }

    Console.Write("Apellido: ");
    var apellido = Console.ReadLine()!.Trim();
    if (string.IsNullOrWhiteSpace(apellido) || !apellido.All(c => char.IsLetter(c) || c == ' '))
    {
        Console.WriteLine("\nEl apellido solo puede contener letras.");
        Console.ReadKey();
        return;
    }

    Console.Write("DNI: ");
    var dni = Console.ReadLine()!.Trim();
    if (!dni.All(char.IsDigit) || dni.Length != 8 || dni.Length == 0)
    {
        Console.WriteLine("\nEl DNI debe contener solo números y tener 8 dígitos.");
        Console.ReadKey();
        return;
    }

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
        Console.WriteLine("\nError al agregar el alumno. Es posible que el DNI ya exista.");

    Console.WriteLine("Presioná cualquier tecla para volver.");
    Console.ReadKey();
}

async Task EditarAlumno()
{
    Console.Clear();
    Console.WriteLine("=== EDITAR ALUMNO ===\n");

    Console.Write("Ingresá el ID del alumno a editar: ");
    if (!int.TryParse(Console.ReadLine(), out int id))
    {
        Console.WriteLine("\nID inválido.");
        Console.ReadKey();
        return;
    }

    var alumno = await client.Alumnos.GetByIdAsync(id);
    if (alumno == null)
    {
        Console.WriteLine("\nNo se encontró el alumno.");
        Console.ReadKey();
        return;
    }

    Console.WriteLine($"\nAlumno actual: {alumno.Apellido}, {alumno.Nombre} - DNI: {alumno.DNI}");
    Console.WriteLine("(Dejá en blanco para mantener el valor actual)\n");

    Console.Write($"Nombre [{alumno.Nombre}]: ");
    var nombre = Console.ReadLine()!.Trim();
    if (string.IsNullOrWhiteSpace(nombre))
        nombre = alumno.Nombre;
    else if (!nombre.All(c => char.IsLetter(c) || c == ' '))
    {
        Console.WriteLine("\nEl nombre solo puede contener letras.");
        Console.ReadKey();
        return;
    }

    Console.Write($"Apellido [{alumno.Apellido}]: ");
    var apellido = Console.ReadLine()!.Trim();
    if (string.IsNullOrWhiteSpace(apellido))
        apellido = alumno.Apellido;
    else if (!apellido.All(c => char.IsLetter(c) || c == ' '))
    {
        Console.WriteLine("\nEl apellido solo puede contener letras.");
        Console.ReadKey();
        return;
    }

    Console.Write($"DNI [{alumno.DNI}]: ");
    var dni = Console.ReadLine()!.Trim();
    if (string.IsNullOrWhiteSpace(dni))
        dni = alumno.DNI;
    else if (!dni.All(char.IsDigit) || dni.Length != 8 || dni.Length == 0)
    {
        Console.WriteLine("\nEl DNI debe contener solo números y debe tener 8 dígitos.");
        Console.ReadKey();
        return;
    }

    var dto = new AlumnoDTO
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
    if (!int.TryParse(Console.ReadLine(), out int id))
    {
        Console.WriteLine("\nID inválido.");
        Console.ReadKey();
        return;
    }

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
    if (!int.TryParse(Console.ReadLine(), out int id))
    {
        Console.WriteLine("\nID inválido.");
        Console.ReadKey();
        return;
    }

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
    if (!int.TryParse(Console.ReadLine(), out int cursoId))
    {
        Console.WriteLine("\nID inválido.");
        Console.ReadKey();
        return;
    }
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

    if (!int.TryParse(Console.ReadLine(), out int cursoId))
    {
        Console.WriteLine("\nID inválido.");
        Console.ReadKey();
        return;
    }

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
    if (!int.TryParse(Console.ReadLine(), out int cursoId))
    {
        Console.WriteLine("\nID inválido.");
        Console.ReadKey();
        return;
    }

    var asistencias = await client.Asistencias.GetByCursoHoyAsync(cursoId);

    var estados = new Dictionary<int, string>
    {
        { 0, "Presente" },
        { 1, "Ausente" },
        { 2, "Tarde" },
        { 3, "Ausente con presencia" },
        { 4, "Ausente justificado" }
    };

    Console.WriteLine();
    if (asistencias.Count == 0)
        Console.WriteLine("No hay asistencias registradas hoy para este curso.");
    else
        foreach (var a in asistencias)
        {
            var estadoTexto = estados.TryGetValue(a.Estado, out var texto) ? texto : "Desconocido";
            Console.WriteLine($"AlumnoID: {a.AlumnoId} - Estado: {estadoTexto} - Fecha: {a.Fecha}");
        }

    Console.WriteLine("\nPresioná cualquier tecla para volver.");
    Console.ReadKey();
}

async Task VerResumenAlumno()
{
    Console.Clear();
    Console.WriteLine("=== RESUMEN DE ASISTENCIAS ===\n");

    Console.Write("Ingresá el ID del alumno: ");
    if (!int.TryParse(Console.ReadLine(), out int id))
    {
        Console.WriteLine("\nID inválido.");
        Console.ReadKey();
        return;
    }

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
        Console.WriteLine("3. Exportar asistencias del mes a Excel");
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
            case "3":
                await ExportarAsistenciasMes();
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
    if (!int.TryParse(Console.ReadLine(), out int cursoId))
    {
        Console.WriteLine("\nID inválido.");
        Console.ReadKey();
        return;
    }

    var archivo = await client.Etl.ExportarAlumnosAsync(cursoId);

    if (archivo == null)
    {
        Console.WriteLine("\nError al exportar.");
        Console.ReadKey();
        return;
    }

    Console.Write("\nIngresá la ruta donde guardar el archivo (ej: C:\\Users\\Usuario\\Desktop\\alumnos.xlsx): ");
    var ruta = Console.ReadLine()!;

    try
    {
        await File.WriteAllBytesAsync(ruta, archivo);
        Console.WriteLine("\nArchivo exportado correctamente.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\nNo se pudo guardar el archivo: {ex.Message}");
    }

    Console.WriteLine("Presioná cualquier tecla para volver.");
    Console.ReadKey();
}
async Task ImportarAlumnos()
{
    Console.Clear();
    Console.WriteLine("=== IMPORTAR ALUMNOS DESDE EXCEL ===\n");

    Console.Write("Ingresá la ruta del archivo Excel: ");
    var ruta = Console.ReadLine()!;

    if (string.IsNullOrWhiteSpace(ruta))
    {
        Console.WriteLine("\nLa ruta no puede estar vacía.");
        Console.ReadKey();
        return;
    }

    if (!File.Exists(ruta))
    {
        Console.WriteLine("\nNo se encontró el archivo en esa ruta.");
        Console.ReadKey();
        return;
    }

    try
    {
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
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\nError al leer el archivo: {ex.Message}");
    }

    Console.WriteLine("Presioná cualquier tecla para volver.");
    Console.ReadKey();
}
async Task ExportarAsistenciasMes()
{
    Console.Clear();
    Console.WriteLine("=== EXPORTAR ASISTENCIAS DEL MES ===\n");

    var cursos = await client.Cursos.GetAllAsync();
    foreach (var c in cursos)
        Console.WriteLine($"[{c.Id}] {c.Nombre} - {c.Anio} / {c.Division}");

    Console.Write("\nIngresá el ID del curso: ");
    if (!int.TryParse(Console.ReadLine(), out int cursoId))
    {
        Console.WriteLine("\nID inválido.");
        Console.ReadKey();
        return;
    }

    Console.Write("Año (ej: 2026): ");
    if (!int.TryParse(Console.ReadLine(), out int anio))
    {
        Console.WriteLine("\nAño inválido.");
        Console.ReadKey();
        return;
    }

    Console.Write("Mes (1-12): ");
    if (!int.TryParse(Console.ReadLine(), out int mes) || mes < 1 || mes > 12)
    {
        Console.WriteLine("\nMes inválido, debe ser un número entre 1 y 12.");
        Console.ReadKey();
        return;
    }

    var archivo = await client.Etl.ExportarAsistenciasMesAsync(cursoId, anio, mes);

    if (archivo == null)
    {
        Console.WriteLine("\nError al exportar. Verificá que el curso exista.");
        Console.ReadKey();
        return;
    }

    Console.Write("\nIngresá la ruta donde guardar el archivo (ej: C:\\Users\\Usuario\\Desktop\\asistencias.xlsx): ");
    var ruta = Console.ReadLine()!;

    try
    {
        await File.WriteAllBytesAsync(ruta, archivo);
        Console.WriteLine("\nArchivo exportado correctamente.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\nNo se pudo guardar el archivo: {ex.Message}");
    }

    Console.WriteLine("Presioná cualquier tecla para volver.");
    Console.ReadKey();
}
#endregion

//Region encargada de manejo de usuarios
#region Usuarios

async Task MenuUsuarios()
{
    bool volver = false;

    while (!volver)
    {
        Console.Clear();
        Console.WriteLine("=== USUARIOS ===\n");
        Console.WriteLine("1. Ver todos los usuarios");
        Console.WriteLine("2. Invitar usuario");
        Console.WriteLine("3. Desactivar usuario");
        Console.WriteLine("4. Reactivar usuario");
        Console.WriteLine("5. Resetear contraseña de un usuario");
        Console.WriteLine("0. Volver\n");
        Console.Write("Elegí una opción: ");

        var opcion = Console.ReadLine();

        switch (opcion)
        {
            case "1":
                await VerUsuarios();
                break;
            case "2":
                await InvitarUsuario();
                break;
            case "3":
                await DesactivarUsuario();
                break;
            case "4":
                await ReactivarUsuario();
                break;
            case "5":
                await ResetearPassword();
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

async Task InvitarUsuario()
{
    Console.Clear();
    Console.WriteLine("=== INVITAR USUARIO ===\n");

    Console.Write("Email: ");
    var email = Console.ReadLine()!.Trim();

    Console.WriteLine("1. Admin  2. Preceptor  3. Directivo");
    Console.Write("Rol: ");
    var rolOpcion = Console.ReadLine();
    var rol = rolOpcion switch
    {
        "1" => "Admin",
        "2" => "Preceptor",
        "3" => "Directivo",
        _ => null
    };
    if (rol == null)
    {
        Console.WriteLine("\nRol inválido.");
        Console.ReadKey();
        return;
    }

    var token = await client.Auth.InvitarUsuarioAsync(email, rol);

    if (token != null)
        Console.WriteLine($"\nInvitación creada. Token de activación:\n\n{token}\n");
    else
        Console.WriteLine("\nError al crear la invitación. El email ya existe o el rol es inválido.");

    Console.WriteLine("Presioná cualquier tecla para volver.");
    Console.ReadKey();
}

async Task DesactivarUsuario()
{
    Console.Clear();
    Console.WriteLine("=== DESACTIVAR USUARIO ===\n");

    Console.Write("Ingresá el ID del usuario a desactivar: ");
    if (!int.TryParse(Console.ReadLine(), out int id))
    {
        Console.WriteLine("\nID inválido.");
        Console.ReadKey();
        return;
    }

    var resultado = await client.Auth.DesactivarUsuarioAsync(id);

    if (resultado)
        Console.WriteLine("\nUsuario desactivado correctamente.");
    else
        Console.WriteLine("\nNo se encontró el usuario.");

    Console.WriteLine("Presioná cualquier tecla para volver.");
    Console.ReadKey();
}

async Task VerUsuarios()
{
    Console.Clear();
    Console.WriteLine("=== USUARIOS ===\n");

    var usuarios = await client.Auth.GetAllUsuariosAsync();

    if (usuarios.Count == 0)
    {
        Console.WriteLine("No hay usuarios registrados.");
    }
    else
    {
        foreach (var u in usuarios)
        {
            var estado = u.EstaActivo ? "Activo" : "Inactivo";
            var username = u.Username ?? "Sin activar";
            Console.WriteLine($"[{u.Id}] {username} - {u.Email} - {u.Rol} - {estado}");
        }
    }

    Console.WriteLine("\nPresioná cualquier tecla para volver.");
    Console.ReadKey();
}

async Task ReactivarUsuario()
{
    Console.Clear();
    Console.WriteLine("=== REACTIVAR USUARIO ===\n");

    Console.Write("Ingresá el ID del usuario a reactivar: ");
    if (!int.TryParse(Console.ReadLine(), out int id))
    {
        Console.WriteLine("\nID inválido.");
        Console.ReadKey();
        return;
    }

    var resultado = await client.Auth.ReactivarUsuarioAsync(id);

    if (resultado)
        Console.WriteLine("\nUsuario reactivado correctamente.");
    else
        Console.WriteLine("\nNo se encontró el usuario.");

    Console.WriteLine("Presioná cualquier tecla para volver.");
    Console.ReadKey();
}

async Task CambiarMiPassword()
{
    Console.Clear();
    Console.WriteLine("=== CAMBIAR CONTRASEÑA ===\n");

    Console.Write("Contraseña actual: ");
    var actual = Console.ReadLine()!;

    Console.Write("Contraseña nueva: ");
    var nueva = Console.ReadLine()!;

    if (string.IsNullOrWhiteSpace(nueva))
    {
        Console.WriteLine("\nLa contraseña nueva no puede estar vacía.");
        Console.ReadKey();
        return;
    }

    var resultado = await client.Auth.CambiarPasswordAsync(actual, nueva);

    if (resultado)
        Console.WriteLine("\nContraseña cambiada correctamente.");
    else
        Console.WriteLine("\nLa contraseña actual es incorrecta.");

    Console.WriteLine("Presioná cualquier tecla para volver.");
    Console.ReadKey();
}

async Task ResetearPassword()
{
    Console.Clear();
    Console.WriteLine("=== RESETEAR CONTRASEÑA ===\n");

    var usuarios = await client.Auth.GetAllUsuariosAsync();
    foreach (var u in usuarios)
    {
        var estado = u.EstaActivo ? "Activo" : "Inactivo";
        var username = u.Username ?? "Sin activar";
        Console.WriteLine($"[{u.Id}] {username} - {u.Email} - {u.Rol} - {estado}");
    }

    Console.Write("\nIngresá el ID del usuario: ");
    if (!int.TryParse(Console.ReadLine(), out int id))
    {
        Console.WriteLine("\nID inválido.");
        Console.ReadKey();
        return;
    }

    var passwordTemporal = await client.Auth.ResetearPasswordAsync(id);

    if (passwordTemporal != null)
    {
        Console.WriteLine($"\nContraseña reseteada correctamente.");
        Console.WriteLine($"Contraseña temporal: {passwordTemporal}");
        Console.WriteLine("Comunicásela al usuario para que pueda ingresar y cambiarla.");
    }
    else
        Console.WriteLine("\nNo se encontró el usuario.");

    Console.WriteLine("Presioná cualquier tecla para volver.");
    Console.ReadKey();
}
#endregion