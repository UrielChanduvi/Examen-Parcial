# Portal Academico - Gestion de Cursos y Matriculas

Portal interno universitario construido con ASP.NET Core MVC 9 + Identity y EF Core sobre SQLite. Este repositorio contiene la resolucion del examen parcial descrito en el enunciado.

## Requisitos previos

- .NET SDK 9.0.202 o superior
- SQLite 3

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

## Estructura actual

- `feature/bootstrap-dominio`: creacion del proyecto base, modelos `Curso` y `Matricula`, restricciones y datos semilla.
- `feature/catalogo-cursos`: catalogo con filtros por nombre, rango de creditos, horario y vista detalle con boton de inscripcion.

El documento se actualizara con las demas preguntas (matriculas, sesiones/Redis, panel coordinador y despliegue en Render) conforme se implementen.
