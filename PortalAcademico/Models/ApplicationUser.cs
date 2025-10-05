using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace PortalAcademico.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Career { get; set; } = string.Empty;

    public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();

    public string FullName => $"{FirstName} {LastName}".Trim();
}
