using System;
using System.Collections.Generic;

namespace PortalAcademico.Models;

public class Curso
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int Creditos { get; set; }
    public int CupoMaximo { get; set; }
    public TimeOnly HorarioInicio { get; set; }
    public TimeOnly HorarioFin { get; set; }
    public bool Activo { get; set; }

    public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();
}
