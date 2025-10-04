using System.Collections.Generic;
using PortalAcademico.Models;

namespace PortalAcademico.ViewModels;

public class CatalogoCursosViewModel
{
    public CatalogoCursosFilterViewModel Filtro { get; set; } = new();
    public IReadOnlyCollection<Curso> Cursos { get; set; } = new List<Curso>();
}
