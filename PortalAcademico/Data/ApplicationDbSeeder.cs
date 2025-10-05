using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PortalAcademico.Models;

namespace PortalAcademico.Data;

public static class ApplicationDbSeeder
{
    private const string CoordinadorRole = "Coordinador";
    private const string EstudianteRole = "Estudiante";
    private const string CoordinadorEmail = "coordinador@universidad.test";
    private const string CoordinadorPassword = "P@ssw0rd!";

    private const string NormalizeIdentityBooleansSql = """
DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE lower(table_name) = 'aspnetusers'
          AND lower(column_name) = 'emailconfirmed'
          AND data_type <> 'boolean'
    ) THEN
        ALTER TABLE "AspNetUsers"
            ALTER COLUMN "EmailConfirmed" TYPE boolean USING CASE WHEN "EmailConfirmed"::text IN ('1', 'true', 't', 'yes', 'y') THEN true ELSE false END,
            ALTER COLUMN "PhoneNumberConfirmed" TYPE boolean USING CASE WHEN "PhoneNumberConfirmed"::text IN ('1', 'true', 't', 'yes', 'y') THEN true ELSE false END,
            ALTER COLUMN "TwoFactorEnabled" TYPE boolean USING CASE WHEN "TwoFactorEnabled"::text IN ('1', 'true', 't', 'yes', 'y') THEN true ELSE false END,
            ALTER COLUMN "LockoutEnabled" TYPE boolean USING CASE WHEN "LockoutEnabled"::text IN ('1', 'true', 't', 'yes', 'y') THEN true ELSE false END;
    END IF;
END
$$;
""";

    private const string NormalizeCursosSql = """
DO $$
DECLARE
    seq_name text := '"Cursos_Id_seq"';
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE lower(table_name) = 'cursos'
          AND lower(column_name) = 'activo'
          AND data_type <> 'boolean'
    ) THEN
        ALTER TABLE "Cursos"
            ALTER COLUMN "Activo" TYPE boolean USING CASE WHEN "Activo"::text IN ('1', 'true', 't', 'yes', 'y') THEN true ELSE false END;
    END IF;

    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE lower(table_name) = 'cursos'
          AND lower(column_name) = 'id'
          AND (column_default IS NULL OR column_default = '')
    ) THEN
        EXECUTE format('CREATE SEQUENCE IF NOT EXISTS %s', seq_name);
        EXECUTE format('ALTER TABLE "Cursos" ALTER COLUMN "Id" SET DEFAULT nextval(''%s'')', seq_name);
        EXECUTE format('SELECT setval(''%s'', COALESCE(MAX("Id"), 0) + 1, false) FROM "Cursos"', seq_name);
    END IF;
END
$$;
""";

    public static async Task SeedAsync(IServiceProvider services, ILogger logger)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();
        await NormalizeSchemaAsync(context, logger);

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        await EnsureRoleAsync(roleManager, logger);
        var coordinator = await EnsureCoordinatorAsync(userManager, logger);
        await EnsureCoursesAsync(context, logger);

        if (coordinator is not null && !await userManager.IsInRoleAsync(coordinator, CoordinadorRole))
        {
            var result = await userManager.AddToRoleAsync(coordinator, CoordinadorRole);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                logger.LogError("Error asignando rol de coordinador: {Errors}", errors);
            }
        }
    }

    private static async Task NormalizeSchemaAsync(ApplicationDbContext context, ILogger logger)
    {
        if (context.Database.ProviderName != "Npgsql.EntityFrameworkCore.PostgreSQL")
        {
            return;
        }

        try
        {
            await context.Database.ExecuteSqlRawAsync(NormalizeIdentityBooleansSql);
            await context.Database.ExecuteSqlRawAsync(NormalizeCursosSql);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "No fue posible normalizar columnas en Postgres");
        }
    }

    private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
    {
        if (await roleManager.RoleExistsAsync(CoordinadorRole))
        {
            return;
        }

        var result = await roleManager.CreateAsync(new IdentityRole(CoordinadorRole));
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            logger.LogError("Error creando rol coordinador: {Errors}", errors);
        }
    }

    private static async Task<ApplicationUser?> EnsureCoordinatorAsync(UserManager<ApplicationUser> userManager, ILogger logger)
    {
        var user = await userManager.Users.FirstOrDefaultAsync(u => u.Email == CoordinadorEmail);
        if (user is not null)
        {
            return user;
        }

        var newUser = new ApplicationUser
        {
            UserName = CoordinadorEmail,
            Email = CoordinadorEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(newUser, CoordinadorPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            logger.LogError("Error creando usuario coordinador: {Errors}", errors);
            return null;
        }

        return newUser;
    }

    private static async Task EnsureCoursesAsync(ApplicationDbContext context, ILogger logger)
    {
        if (await context.Cursos.AnyAsync())
        {
            return;
        }

        var cursos = new[]
        {
            new Curso
            {
                Codigo = "CS101",
                Nombre = "Introduccion a la Programacion",
                Creditos = 4,
                CupoMaximo = 30,
                HorarioInicio = new TimeOnly(8, 0),
                HorarioFin = new TimeOnly(10, 0),
                Activo = true
            },
            new Curso
            {
                Codigo = "CS205",
                Nombre = "Bases de Datos",
                Creditos = 3,
                CupoMaximo = 25,
                HorarioInicio = new TimeOnly(10, 0),
                HorarioFin = new TimeOnly(12, 0),
                Activo = true
            },
            new Curso
            {
                Codigo = "NET301",
                Nombre = "Redes y Comunicaciones",
                Creditos = 3,
                CupoMaximo = 20,
                HorarioInicio = new TimeOnly(14, 0),
                HorarioFin = new TimeOnly(16, 0),
                Activo = true
            }
        };

        await context.Cursos.AddRangeAsync(cursos);
        await context.SaveChangesAsync();
        logger.LogInformation("Cursos iniciales creados");
    }
}
