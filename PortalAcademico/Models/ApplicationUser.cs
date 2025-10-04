using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace PortalAcademico.Models;

public class ApplicationUser : IdentityUser
{
    public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();
}
