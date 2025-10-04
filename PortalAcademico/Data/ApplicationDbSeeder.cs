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
    private const string CoordinadorEmail = "coordinador@universidad.test";
    private const string CoordinadorPassword = "P@ssw0rd!";

    public static async Task SeedAsync(IServiceProvider services, ILogger logger)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

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
