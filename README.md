# Gestión Académica

Sistema de gestión académica desarrollado en .NET 8 que permite administrar cursos, alumnos y asistencias. La solución está dividida en tres proyectos que trabajan juntos: una API REST, una librería cliente y una aplicación de consola.

---

## Tecnologías y librerías

| Tecnología | Uso |
|---|---|
| ASP.NET Core 8 | Backend / API REST |
| Entity Framework Core 8 | ORM para la base de datos |
| PostgreSQL | Motor de base de datos |
| C# | Lenguaje principal |
| BCrypt.Net-Next | Hashing de contraseñas |
| Microsoft.AspNetCore.Authentication.JwtBearer 8 | Autenticación JWT |
| ClosedXML | Exportación e importación de Excel |
| System.IdentityModel.Tokens.Jwt | Lectura de tokens JWT en la consola |
| Microsoft.Extensions.Configuration.Json | Configuración por archivo en la consola |

---

## Arquitectura

La solución está compuesta por tres proyectos:

```
GestionAcademica.sln
├── GestionApi/           → API REST (lógica de negocio + base de datos)
├── GestionApiClient/     → Librería puente (HTTP + JSON)
└── GestionConsole/       → Aplicación de consola (interfaz de usuario)
```

El flujo de comunicación es el siguiente:

```
Usuario
   ↓ escribe en la consola
GestionConsole
   ↓ llama métodos C#
GestionApiClient
   ↓ HTTP + JSON + JWT
GestionApi
   ↓ Entity Framework
PostgreSQL
```

La consola nunca maneja HTTP ni JSON directamente. Todo eso queda encapsulado en el ApiClient, que expone métodos simples en C#.

---

## DER

![DER](images/der.jpeg)

### Entidades

**Cursos**
- `Id` — identificador único
- `Nombre` — nombre del curso
- `Anio` — año del curso
- `Division` — división (ej: "A", "B")
- Restricción única: `Anio + Division`

**Alumnos**
- `Id` — identificador único
- `Nombre`, `Apellido` — solo letras, obligatorios
- `DNI` — único en el sistema, solo números, máximo 8 dígitos
- `CursoId` (FK nullable) — curso asignado, null si no está activo
- `Estado` — `Activo` / `ExAlumno` / `Egresado`
- `FechaBaja` — se completa al cambiar de estado

**Asistencias**
- `Id` — identificador único
- `AlumnoId` (FK), `CursoId` (FK)
- `Fecha` — fecha de la asistencia
- `Estado` — `Presente` / `Ausente` / `Tarde` / `AusenteConPresencia` / `AusenteJustificado`
- Restricción única: `AlumnoId + Fecha`

**Usuarios**
- `Id` — identificador único
- `Username` (único, nullable hasta la activación)
- `Email` — único
- `PasswordHash` — hasheado con BCrypt
- `Rol` — `Admin` / `Preceptor` / `Directivo`
- `EstaActivo` — false hasta que el usuario active su cuenta
- `TokenActivacion` — GUID generado al invitar

---

## Sistema de autenticación

El sistema usa JWT con un ciclo de vida de tres etapas:

1. **Seed inicial** — al crear la base de datos se genera un usuario `admin` con contraseña `admin123`
2. **Invitación** — el Admin crea una invitación con email y rol, la API genera un token de activación que se le entrega al nuevo usuario
3. **Activación** — el nuevo usuario ingresa el token, define su username y contraseña

Cuando el Admin resetea la contraseña de un usuario, el sistema genera una contraseña temporal automáticamente y se la muestra al Admin para que se la comunique al usuario.

### Matriz de permisos

| Funcionalidad | Admin | Directivo | Preceptor |
|---|---|---|---|
| CRUD Cursos | si | si | no |
| Ver alumnos | si | si | si |
| Agregar/Editar alumnos | si | si | no |
| Dar de baja / Egresar | si | si | no |
| Registrar asistencia | si | si | si |
| Editar asistencia | si | si | si |
| Ver resumen asistencias | si | si | si |
| Exportar / Importar Excel | si | si | no |
| Gestionar usuarios | si | no | no |
| Cambiar propia contraseña | si | si | si |

---

## Endpoints de la API

### Autenticación y usuarios
| Método | URL | Descripción |
|---|---|---|
| POST | `/api/auth/login` | Iniciar sesión |
| POST | `/api/auth/invitar` | Invitar nuevo usuario (Admin) |
| POST | `/api/auth/activar` | Activar cuenta con token |
| GET | `/api/auth/usuarios` | Ver todos los usuarios (Admin) |
| PATCH | `/api/auth/cambiar-password` | Cambiar propia contraseña |
| PATCH | `/api/auth/usuarios/{id}/desactivar` | Desactivar usuario (Admin) |
| PATCH | `/api/auth/usuarios/{id}/reactivar` | Reactivar usuario (Admin) |
| PATCH | `/api/auth/usuarios/{id}/resetear-password` | Resetear contraseña (Admin) |

### Cursos
| Método | URL | Descripción |
|---|---|---|
| GET | `/api/cursos` | Obtener todos los cursos |
| GET | `/api/cursos/{id}` | Obtener un curso por ID |
| POST | `/api/cursos` | Crear un curso |
| PUT | `/api/cursos/{id}` | Editar un curso |
| DELETE | `/api/cursos/{id}` | Eliminar un curso |

### Alumnos
| Método | URL | Descripción |
|---|---|---|
| GET | `/api/alumnos` | Obtener todos los alumnos (paginado) |
| GET | `/api/cursos/{id}/alumnos` | Obtener alumnos activos de un curso |
| GET | `/api/alumnos/exalumnos` | Obtener todos los ex-alumnos |
| GET | `/api/alumnos/egresados` | Obtener todos los egresados |
| GET | `/api/alumnos/{id}` | Obtener un alumno por ID |
| POST | `/api/alumnos` | Agregar alumno |
| PUT | `/api/alumnos/{id}` | Editar alumno |
| PATCH | `/api/alumnos/{id}/baja` | Dar de baja un alumno |
| PATCH | `/api/alumnos/{id}/egresar` | Marcar alumno como egresado |
| PATCH | `/api/alumnos/{alumnoId}/mover/{cursoId}` | Mover alumno a otro curso |

### Asistencias
| Método | URL | Descripción |
|---|---|---|
| POST | `/api/asistencias/bulk` | Registrar asistencias del día |
| PUT | `/api/asistencias/{id}` | Editar una asistencia |
| GET | `/api/asistencias/curso/{id}/hoy` | Ver asistencias de un curso hoy |
| GET | `/api/asistencias/alumno/{id}/resumen` | Ver resumen de asistencias de un alumno |

### ETL
| Método | URL | Descripción |
|---|---|---|
| GET | `/api/etl/exportar/alumnos/{cursoId}` | Exportar alumnos a Excel |
| GET | `/api/etl/exportar/asistencias/{cursoId}/{anio}/{mes}` | Exportar asistencias del mes a Excel |
| POST | `/api/etl/importar/alumnos` | Importar alumnos desde Excel |

---

## Reglas de negocio

- No pueden existir dos cursos con el mismo año y división
- No pueden existir dos alumnos con el mismo DNI
- El DNI solo puede contener números y tener como máximo 8 dígitos
- El nombre y apellido solo pueden contener letras
- Un alumno no puede tener dos registros de asistencia en la misma fecha
- Un curso con alumnos activos no puede eliminarse
- Los alumnos nunca se eliminan físicamente — su estado cambia mediante el enum `EstadoAlumno`
- Al dar de baja o egresar: `CursoId = null`, `FechaBaja = hoy`
- Un usuario desactivado no puede iniciar sesión
- Al resetear la contraseña el sistema genera una contraseña temporal automáticamente

---

## Importar alumnos desde Excel

El archivo debe tener la siguiente estructura:

| Nombre | Apellido | DNI | CursoId (opcional) |
|--------|----------|-----|--------------------|
| Juan | García | 12345678 | 1 |
| María | López | 87654321 | |

- Si el `CursoId` no existe o está vacío, el alumno se importa sin curso asignado
- Si el DNI ya existe en el sistema, ese alumno se saltea y se continúa con los demás
- Al finalizar la importación se muestra un resumen con el resultado de cada fila

---

## Requisitos previos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- [PostgreSQL](https://www.postgresql.org/download/)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) o superior

---

## Cómo ejecutar la aplicación

### 1. Clonar el repositorio

```bash
git clone https://github.com/ValentinBlacutt/Gestion-Academica.git
```

### 2. Configurar la base de datos

Abrí `GestionApi/appsettings.json` y reemplazá los datos de conexión:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=gestion_academica;Username=postgres;Password=TU_CONTRASEÑA"
  },
  "Jwt": {
    "SecretKey": "esta-es-una-clave-secreta-muy-larga-y-segura-2026",
    "Issuer": "GestionAcademica",
    "Audience": "GestionAcademica",
    "ExpirationHours": 4
  }
}
```

### 3. Crear la base de datos

Desde la terminal parado en la carpeta `GestionApi`:

```bash
dotnet ef database update
```

Esto crea la base de datos con todas las tablas y el usuario `admin` inicial.

### 4. Configurar el puerto de la API

Corré la API una vez para ver el puerto asignado en la terminal. Luego abrí `GestionConsole/appsettings.json` y actualizá:

```json
{
  "ApiBaseUrl": "https://localhost:TU_PUERTO/"
}
```

### 5. Ejecutar la aplicación

En Visual Studio:
1. Clic derecho en la solución → Properties
2. Common Properties → Startup Project → Multiple startup projects
3. Configurá GestionApi y GestionConsole con acción Start
4. Presioná F5

### 6. Credenciales iniciales

```
Usuario: admin
Contraseña: admin123
```

Se recomienda cambiar la contraseña del administrador después del primer login.

---

## Estructura del proyecto

```
GestionAcademica/
├── GestionApi/
│   ├── Controllers/       → Endpoints REST
│   ├── Services/          → Lógica de negocio
│   ├── Models/            → Entidades de la base de datos
│   ├── DTOs/              → Objetos de transferencia de datos
│   ├── Enums/             → Enumeraciones del sistema
│   ├── Data/              → DbContext y configuración EF Core
│   └── Program.cs         → Configuración de la aplicación
├── GestionApiClient/
│   ├── Clients/           → Clientes HTTP por entidad
│   ├── DTOs/              → DTOs del cliente
│   └── GestionApiClient.cs → Punto de entrada del cliente
├── GestionConsole/
│   ├── appsettings.json   → URL de la API
│   └── Program.cs         → Menú e interfaz de usuario
└── images/
    └── der.jpeg           → Diagrama entidad-relación
```
