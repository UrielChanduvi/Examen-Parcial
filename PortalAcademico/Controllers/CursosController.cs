using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;
using PortalAcademico.ViewModels;

namespace PortalAcademico.Controllers;

public class CursosController : Controller
{
    private readonly ApplicationDbContext _context;

    public CursosController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] CatalogoCursosFilterViewModel filtro)
    {
        var filtroAplicado = ModelState.IsValid ? filtro : new CatalogoCursosFilterViewModel();

        var query = _context.Cursos.AsNoTracking().Where(c => c.Activo);

        if (!string.IsNullOrWhiteSpace(filtroAplicado.Nombre))
        {
            var nombre = filtroAplicado.Nombre!.Trim();
            query = query.Where(c => EF.Functions.Like(c.Nombre, $"%{nombre}%") || EF.Functions.Like(c.Codigo, $"%{nombre}%"));
        }

        if (filtroAplicado.CreditosMin is not null)
        {
            query = query.Where(c => c.Creditos >= filtroAplicado.CreditosMin);
        }

        if (filtroAplicado.CreditosMax is not null)
        {
            query = query.Where(c => c.Creditos <= filtroAplicado.CreditosMax);
        }

        if (filtroAplicado.HorarioInicio is not null)
        {
            query = query.Where(c => c.HorarioInicio >= filtroAplicado.HorarioInicio);
        }

        if (filtroAplicado.HorarioFin is not null)
        {
            query = query.Where(c => c.HorarioFin <= filtroAplicado.HorarioFin);
        }

        var cursos = await query.OrderBy(c => c.HorarioInicio).ThenBy(c => c.Nombre).ToListAsync();

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

        return View(curso);
    }
}
