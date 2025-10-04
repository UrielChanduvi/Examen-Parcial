using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;
using PortalAcademico.Models;

namespace PortalAcademico.Controllers;

[Authorize]
public class MatriculasController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public MatriculasController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int cursoId)
    {
        var curso = await _context.Cursos.AsNoTracking().FirstOrDefaultAsync(c => c.Id == cursoId && c.Activo);
        if (curso is null)
        {
            return NotFound();
        }

        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        var yaInscrito = await _context.Matriculas.AnyAsync(m => m.CursoId == cursoId && m.UsuarioId == userId && m.Estado != MatriculaEstado.Cancelada);
        if (yaInscrito)
        {
            TempData["ErrorMessage"] = "Ya tienes una matricula activa para este curso.";
            return RedirectToAction("Details", "Cursos", new { id = cursoId });
        }

        var cupoOcupado = await _context.Matriculas.CountAsync(m => m.CursoId == cursoId && m.Estado != MatriculaEstado.Cancelada);
        if (cupoOcupado >= curso.CupoMaximo)
        {
            TempData["ErrorMessage"] = "No es posible inscribirse: el curso alcanzo su cupo maximo.";
            return RedirectToAction("Details", "Cursos", new { id = cursoId });
        }

        var cursosUsuario = await _context.Matriculas
            .Include(m => m.Curso)
            .Where(m => m.UsuarioId == userId && m.Estado != MatriculaEstado.Cancelada)
            .Select(m => m.Curso)
            .Where(c => c != null && c.Activo)
            .ToListAsync();

        var solapaHorario = cursosUsuario.Any(c => c != null && c.HorarioInicio < curso.HorarioFin && curso.HorarioInicio < c.HorarioFin);
        if (solapaHorario)
        {
            TempData["ErrorMessage"] = "Ya tienes un curso en el mismo horario.";
            return RedirectToAction("Details", "Cursos", new { id = cursoId });
        }

        var matricula = new Matricula
        {
            CursoId = cursoId,
            UsuarioId = userId,
            FechaRegistro = DateTime.UtcNow,
            Estado = MatriculaEstado.Pendiente
        };

        _context.Matriculas.Add(matricula);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Inscripcion registrada en estado Pendiente.";
        return RedirectToAction("Details", "Cursos", new { id = cursoId });
    }
}
