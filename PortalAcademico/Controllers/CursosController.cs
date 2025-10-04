using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;
using PortalAcademico.Services;
using PortalAcademico.ViewModels;

namespace PortalAcademico.Controllers;

public class CursosController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ICatalogoCursosCacheService _catalogoCursosCache;

    public CursosController(ApplicationDbContext context, ICatalogoCursosCacheService catalogoCursosCache)
    {
        _context = context;
        _catalogoCursosCache = catalogoCursosCache;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] CatalogoCursosFilterViewModel filtro)
    {
        var filtroAplicado = ModelState.IsValid ? filtro : new CatalogoCursosFilterViewModel();

        var cursosActivos = await _catalogoCursosCache.ObtenerCursosActivosAsync();
        var cursosFiltrados = cursosActivos.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(filtroAplicado.Nombre))
        {
            var nombre = filtroAplicado.Nombre.Trim();
            cursosFiltrados = cursosFiltrados.Where(c =>
                c.Nombre.Contains(nombre, StringComparison.OrdinalIgnoreCase) ||
                c.Codigo.Contains(nombre, StringComparison.OrdinalIgnoreCase));
        }

        if (filtroAplicado.CreditosMin is not null)
        {
            cursosFiltrados = cursosFiltrados.Where(c => c.Creditos >= filtroAplicado.CreditosMin);
        }

        if (filtroAplicado.CreditosMax is not null)
        {
            cursosFiltrados = cursosFiltrados.Where(c => c.Creditos <= filtroAplicado.CreditosMax);
        }

        if (filtroAplicado.HorarioInicio is not null)
        {
            cursosFiltrados = cursosFiltrados.Where(c => c.HorarioInicio >= filtroAplicado.HorarioInicio);
        }

        if (filtroAplicado.HorarioFin is not null)
        {
            cursosFiltrados = cursosFiltrados.Where(c => c.HorarioFin <= filtroAplicado.HorarioFin);
        }

        var cursos = cursosFiltrados
            .OrderBy(c => c.HorarioInicio)
            .ThenBy(c => c.Nombre)
            .ToList();

        var viewModel = new CatalogoCursosViewModel
        {
            Filtro = filtro,
            Cursos = cursos
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var curso = await _context.Cursos.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id && c.Activo);
        if (curso is null)
        {
            return NotFound();
        }

        HttpContext.Session.SetInt32("LastCourseId", curso.Id);
        HttpContext.Session.SetString("LastCourseName", curso.Nombre);

        return View(curso);
    }
}
