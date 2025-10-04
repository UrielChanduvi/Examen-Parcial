using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PortalAcademico.ViewModels;

public class CursoFormViewModel : IValidatableObject
{
    public int? Id { get; set; }

    [Required]
    [StringLength(20)]
    public string Codigo { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [Display(Name = "Creditos")]
    [Range(1, int.MaxValue, ErrorMessage = "Los creditos deben ser mayores a cero.")]
    public int Creditos { get; set; }

    [Display(Name = "Cupo maximo")]
    [Range(1, int.MaxValue, ErrorMessage = "El cupo maximo debe ser mayor a cero.")]
    public int CupoMaximo { get; set; }

    [Display(Name = "Horario inicio")]
    [DataType(DataType.Time)]
    [Required]
    public TimeOnly? HorarioInicio { get; set; }

    [Display(Name = "Horario fin")]
    [DataType(DataType.Time)]
    [Required]
    public TimeOnly? HorarioFin { get; set; }

    public bool Activo { get; set; } = true;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (HorarioInicio is not null && HorarioFin is not null && HorarioInicio >= HorarioFin)
        {
            yield return new ValidationResult("El horario de fin debe ser mayor al horario de inicio.", new[] { nameof(HorarioFin) });
        }
    }
}
