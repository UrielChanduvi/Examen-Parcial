using System;

namespace PortalAcademico.Models;

public class Matricula
{
    public int Id { get; set; }
    public int CursoId { get; set; }
    public string UsuarioId { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }
    public MatriculaEstado Estado { get; set; }

    public Curso Curso { get; set; } = null!;
    public ApplicationUser Usuario { get; set; } = null!;
}

public enum MatriculaEstado
{
    Pendiente = 0,
    Confirmada = 1,
    Cancelada = 2
}
