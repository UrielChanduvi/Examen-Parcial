# Portal Academico - Gestion de Cursos y Matriculas

Portal interno universitario construido con ASP.NET Core MVC 9 + Identity y EF Core sobre SQLite. Este repositorio contiene la resolucion del examen parcial descrito en el enunciado.

## Requisitos previos

- .NET SDK 9.0.202 o superior (para desarrollo local)
- Docker Desktop (para construir la imagen de despliegue)
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

- `ConnectionStrings__DefaultConnection`: cadena para la base de datos (SQLite, Postgres u otra).
- `ConnectionStrings__Redis` o `Redis__ConnectionString`: cadena de conexion hacia Redis gestionado.
- `ASPNETCORE_ENVIRONMENT`: `Development` para local, `Production` en despliegue.
- `ASPNETCORE_URLS`: en Render debe apuntar a `http://0.0.0.0:${PORT}` (ya definido en `render.yaml`).

Si no se define una cadena de Redis, la aplicacion usa un cache distribuido en memoria solo para desarrollo.

## Estructura actual

- `feature/bootstrap-dominio`: creacion del proyecto base, modelos `Curso` y `Matricula`, restricciones y datos semilla.
- `feature/catalogo-cursos`: catalogo con filtros por nombre, rango de creditos, horario y vista detalle con boton de inscripcion.
- `feature/matriculas`: flujo de inscripcion con validaciones de autenticacion, cupo y choque de horario, feedback en la vista.
- `feature/sesion-redis`: sesiones respaldadas por Redis para recordar el ultimo curso visitado y cache del catalogo (60s) con invalidacion preparada para operaciones de cursos.
- `feature/panel-coordinador`: panel protegido por rol con CRUD de cursos, desactivacion, gestion de matriculas (confirmar/cancelar) e invalidacion de cache.
- `deploy/render`: configuracion de despliegue en Render (`render.yaml`, `Dockerfile`) y documentacion asociada.

## Despliegue en Render (Docker)

1. Conecta el repositorio en Render y selecciona la rama `deploy/render`. Render leera `render.yaml` y creara un servicio web basado en Docker.
2. El build usara el `Dockerfile` multi-stage para publicar la aplicacion (`dotnet publish`) y ejecutar sobre la imagen `mcr.microsoft.com/dotnet/aspnet:9.0`.
3. Define las variables de entorno requeridas en el panel de Render:
   - `ConnectionStrings__DefaultConnection`: cadena de la base de datos externa (ej. Render Postgres).
   - `Redis__ConnectionString`: cadena provista por RedisLabs/Redis Cloud.
   - `ASPNETCORE_ENVIRONMENT=Production` y `ASPNETCORE_URLS=http://0.0.0.0:${PORT}` (preconfigurado en `render.yaml`).
4. Tras el primer despliegue, utiliza el shell del servicio para ejecutar `dotnet ef database update` apuntando a la base de produccion.
5. Verifica manualmente:
   - Inicio de sesion con `coordinador@universidad.test` / `P@ssw0rd!`.
   - Catalogo, filtros e inscripciones con validaciones.
   - Panel de coordinador: CRUD de cursos y gestion de matriculas.

Para cambios futuros, Render reconstruira la imagen mediante el `Dockerfile` y redeplegara automaticamente.
