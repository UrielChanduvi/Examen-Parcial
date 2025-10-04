using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;
using PortalAcademico.Models;
using PortalAcademico.Services;
using PortalAcademico.ViewModels;

namespace PortalAcademico.Controllers;

[Authorize(Roles = "Coordinador")]
public class CoordinadorController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ICatalogoCursosCacheService _catalogoCache;

    public CoordinadorController(ApplicationDbContext context, ICatalogoCursosCacheService catalogoCache)
    {
        _context = context;
        _catalogoCache = catalogoCache;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return RedirectToAction(nameof(Cursos));
    }

    [HttpGet]
    public async Task<IActionResult> Cursos()
    {
        var cursos = await _context.Cursos
            .AsNoTracking()
            .OrderByDescending(c => c.Activo)
            .ThenBy(c => c.Nombre)
            .ToListAsync();

        return View(cursos);
    }

    [HttpGet]
    public IActionResult Crear()
    {
        return View(new CursoFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(CursoFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var existeCodigo = await _context.Cursos.AnyAsync(c => c.Codigo == model.Codigo);
        if (existeCodigo)
        {
            ModelState.AddModelError(nameof(model.Codigo), "Ya existe un curso con el mismo codigo.");
            return View(model);
        }

        var curso = new Curso
        {
            Codigo = model.Codigo,
            Nombre = model.Nombre,
            Creditos = model.Creditos,
            CupoMaximo = model.CupoMaximo,
            HorarioInicio = model.HorarioInicio!.Value,
            HorarioFin = model.HorarioFin!.Value,
            Activo = model.Activo
        };

        _context.Cursos.Add(curso);
        await _context.SaveChangesAsync();
        await _catalogoCache.InvalidateAsync();

        TempData["SuccessMessage"] = "Curso creado correctamente.";
        return RedirectToAction(nameof(Cursos));
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var curso = await _context.Cursos.FindAsync(id);
        if (curso is null)
        {
            return NotFound();
        }

        var model = new CursoFormViewModel
        {
            Id = curso.Id,
            Codigo = curso.Codigo,
            Nombre = curso.Nombre,
            Creditos = curso.Creditos,
            CupoMaximo = curso.CupoMaximo,
            HorarioInicio = curso.HorarioInicio,
            HorarioFin = curso.HorarioFin,
            Activo = curso.Activo
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, CursoFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var curso = await _context.Cursos.FindAsync(id);
        if (curso is null)
        {
            return NotFound();
        }

        var codigoDuplicado = await _context.Cursos.AnyAsync(c => c.Id != id && c.Codigo == model.Codigo);
        if (codigoDuplicado)
        {
            ModelState.AddModelError(nameof(model.Codigo), "Ya existe otro curso con el mismo codigo.");
            return View(model);
        }

        curso.Codigo = model.Codigo;
        curso.Nombre = model.Nombre;
        curso.Creditos = model.Creditos;
        curso.CupoMaximo = model.CupoMaximo;
        curso.HorarioInicio = model.HorarioInicio!.Value;
        curso.HorarioFin = model.HorarioFin!.Value;
        curso.Activo = model.Activo;

        await _context.SaveChangesAsync();
        await _catalogoCache.InvalidateAsync();

        TempData["SuccessMessage"] = "Curso actualizado correctamente.";
        return RedirectToAction(nameof(Cursos));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Desactivar(int id)
    {
        var curso = await _context.Cursos.FindAsync(id);
        if (curso is null)
        {
            return NotFound();
        }

        curso.Activo = false;
        await _context.SaveChangesAsync();
        await _catalogoCache.InvalidateAsync();

        TempData["SuccessMessage"] = "Curso desactivado.";
        return RedirectToAction(nameof(Cursos));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activar(int id)
    {
        var curso = await _context.Cursos.FindAsync(id);
        if (curso is null)
        {
            return NotFound();
        }

        curso.Activo = true;
        await _context.SaveChangesAsync();
        await _catalogoCache.InvalidateAsync();

        TempData["SuccessMessage"] = "Curso activado.";
        return RedirectToAction(nameof(Cursos));
    }

    [HttpGet]
    public async Task<IActionResult> Matriculas(int id)
    {
        var curso = await _context.Cursos.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        if (curso is null)
        {
            return NotFound();
        }

        var matriculas = await _context.Matriculas
            .AsNoTracking()
            .Where(m => m.CursoId == id)
            .OrderByDescending(m => m.FechaRegistro)
            .Select(m => new MatriculasCursoViewModel.MatriculaItemViewModel(
                m.Id,
                m.Usuario.Email ?? "(sin email)",
                m.FechaRegistro,
                m.Estado))
            .ToListAsync();

        var viewModel = new MatriculasCursoViewModel
        {
            CursoId = curso.Id,
            CursoNombre = curso.Nombre,
            CursoActivo = curso.Activo,
            CupoMaximo = curso.CupoMaximo,
            Matriculas = matriculas
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarMatricula(int id)
    {
        var matricula = await _context.Matriculas.Include(m => m.Curso).FirstOrDefaultAsync(m => m.Id == id);
        if (matricula is null)
        {
            return NotFound();
        }

        matricula.Estado = MatriculaEstado.Confirmada;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Matricula confirmada.";
        return RedirectToAction(nameof(Matriculas), new { id = matricula.CursoId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelarMatricula(int id)
    {
        var matricula = await _context.Matriculas.Include(m => m.Curso).FirstOrDefaultAsync(m => m.Id == id);
        if (matricula is null)
        {
            return NotFound();
        }

        matricula.Estado = MatriculaEstado.Cancelada;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Matricula cancelada.";
        return RedirectToAction(nameof(Matriculas), new { id = matricula.CursoId });
    }
}
