# Portal Academico - Gestion de Cursos y Matriculas

Portal interno universitario construido con ASP.NET Core MVC 9 + Identity y EF Core sobre SQLite. Este repositorio contiene la resolucion del examen parcial descrito en el enunciado.

## Requisitos previos

- .NET SDK 9.0.202 o superior
- SQLite 3
- Instancia de Redis disponible (local o gestionada) para sesiones/cache

## Puesta en marcha local

```bash
cd PortalAcademico
 dotnet restore
 dotnet ef database update
 dotnet run
```

La aplicacion semilla un usuario coordinador al iniciar:

- Usuario: `coordinador@universidad.test`
- Contrasena: `P@ssw0rd!`

## Variables de entorno

Configura las siguientes variables antes de ejecutar en Production o Render:

- `ConnectionStrings__DefaultConnection`: cadena para la base de datos (SQLite/otro proveedor).
- `ConnectionStrings__Redis` o `Redis__ConnectionString`: cadena de conexion hacia Redis.
- `ASPNETCORE_ENVIRONMENT`: `Development` para local, `Production` en despliegue.

Si no se define una cadena de Redis, la aplicacion usa un cache distribuido en memoria solo para desarrollo.

## Estructura actual

- `feature/bootstrap-dominio`: creacion del proyecto base, modelos `Curso` y `Matricula`, restricciones y datos semilla.
- `feature/catalogo-cursos`: catalogo con filtros por nombre, rango de creditos, horario y vista detalle con boton de inscripcion.
- `feature/matriculas`: flujo de inscripcion con validaciones de autenticacion, cupo y choque de horario, feedback en la vista.
- `feature/sesion-redis`: sesiones respaldadas por Redis para recordar el ultimo curso visitado y cache del catalogo (60s) con invalidacion preparada para operaciones de cursos.

El documento se actualizara con las demas preguntas (panel coordinador y despliegue en Render) conforme se implementen.
