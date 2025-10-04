using System;
using System.Collections.Generic;
using System.Linq;
using PortalAcademico.Models;

namespace PortalAcademico.ViewModels;

public class MatriculasCursoViewModel
{
    public int CursoId { get; set; }
    public string CursoNombre { get; set; } = string.Empty;
    public bool CursoActivo { get; set; }
    public int CupoMaximo { get; set; }
    public IReadOnlyCollection<MatriculaItemViewModel> Matriculas { get; set; } = Array.Empty<MatriculaItemViewModel>();

    public int MatriculasPendientes => Matriculas.Count(m => m.Estado == MatriculaEstado.Pendiente);
    public int MatriculasConfirmadas => Matriculas.Count(m => m.Estado == MatriculaEstado.Confirmada);

    public record MatriculaItemViewModel(int Id, string EstudianteEmail, DateTime FechaRegistro, MatriculaEstado Estado);
}
