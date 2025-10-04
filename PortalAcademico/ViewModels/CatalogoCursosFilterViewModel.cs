using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PortalAcademico.ViewModels;

public class CatalogoCursosFilterViewModel : IValidatableObject
{
    [Display(Name = "Nombre del curso")]
    public string? Nombre { get; set; }

    [Display(Name = "Creditos minimos")]
    public int? CreditosMin { get; set; }

    [Display(Name = "Creditos maximos")]
    public int? CreditosMax { get; set; }

    [Display(Name = "Horario desde")]
    [DataType(DataType.Time)]
    public TimeOnly? HorarioInicio { get; set; }

    [Display(Name = "Horario hasta")]
    [DataType(DataType.Time)]
    public TimeOnly? HorarioFin { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CreditosMin is < 0)
        {
            yield return new ValidationResult("El credito minimo no puede ser negativo.", new[] { nameof(CreditosMin) });
        }

        if (CreditosMax is < 0)
        {
            yield return new ValidationResult("El credito maximo no puede ser negativo.", new[] { nameof(CreditosMax) });
        }

        if (CreditosMin is not null && CreditosMax is not null && CreditosMin > CreditosMax)
        {
            yield return new ValidationResult("El credito minimo no puede ser mayor al maximo.", new[] { nameof(CreditosMin), nameof(CreditosMax) });
        }

        if (HorarioInicio is not null && HorarioFin is not null && HorarioInicio > HorarioFin)
        {
            yield return new ValidationResult("El horario final debe ser posterior al inicial.", new[] { nameof(HorarioFin) });
        }
    }
}
