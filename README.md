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

Se ampliara esta documentacion conforme se implementen las siguientes preguntas (catalogo, matriculas, sesiones/Redis, panel de coordinador y despliegue en Render).
