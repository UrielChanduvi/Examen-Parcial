using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using PortalAcademico.Models;

namespace PortalAcademico.Areas.Identity.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<RegisterModel> _logger;

    public RegisterModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<RegisterModel> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public IList<AuthenticationScheme> ExternalLogins { get; set; } = new List<AuthenticationScheme>();

    public string? ReturnUrl { get; set; }

    public IEnumerable<SelectListItem> CareerOptions => StudentCareers.Select(c => new SelectListItem { Text = c, Value = c });

    private static readonly string[] StudentCareers = new[]
    {
        "Ingeniería de Computación y Sistemas",
        "Ingeniería Industrial",
        "Ingeniería Civil",
        "Arquitectura",
        "Ciencias Aeronáuticas"
    };

    public class InputModel
    {
        [Required]
        [Display(Name = "Nombre(s)")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Apellidos")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Correo institucional ( @usmp.pe )")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Carrera")]
        public string Career { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} y máximo {1} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        [Compare("Password", ErrorMessage = "La contraseña y la confirmación no coinciden.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public async Task OnGetAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("/Cursos");
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        if (!Input.Email.EndsWith("@usmp.pe", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError("Input.Email", "El correo debe pertenecer al dominio @usmp.pe.");
        }

        if (ModelState.IsValid)
        {
            var user = new ApplicationUser
            {
                UserName = Input.Email,
                Email = Input.Email,
                EmailConfirmed = true,
                FirstName = Input.FirstName,
                LastName = Input.LastName,
                Career = Input.Career
            };
            var result = await _userManager.CreateAsync(user, Input.Password);
            if (result.Succeeded)
            {
                _logger.LogInformation("Usuario creado con una nueva cuenta.");
                await _userManager.AddToRoleAsync(user, "Estudiante");
                await _signInManager.SignInAsync(user, isPersistent: false);
                return LocalRedirect(returnUrl);
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return Page();
    }
}
