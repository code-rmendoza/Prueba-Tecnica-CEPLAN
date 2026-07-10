using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using PruebaAspNet.Constants;
using PruebaAspNet.Repositories;
using PruebaAspNet.Services;
using PruebaAspNet.ViewModels;

namespace PruebaAspNet.Controllers;

/// <summary>
/// Controlador responsable de la autenticación: Login y Logout.
/// </summary>
public class AccountController : Controller
{
    private readonly IAuthService _authService;
    private readonly IEmailService _emailService;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        IAuthService authService,
        IEmailService emailService,
        IUsuarioRepository usuarioRepository,
        IConfiguration configuration,
        ILogger<AccountController> logger)
    {
        _authService = authService;
        _emailService = emailService;
        _usuarioRepository = usuarioRepository;
        _configuration = configuration;
        _logger = logger;
    }

    // ── GET: /Account/Activate ─────────────────────────────────────────
    [HttpGet]
    public IActionResult Activate()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Profile");

        return View(new ActivateViewModel());
    }

    // ── POST: /Account/Activate ────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(ActivateViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var user = await _usuarioRepository.GetByDocumentAsync(model.TipoDocumento, model.NumeroDocumento);
            if (user == null)
            {
                TempData["ActivateError"] = "El número de documento ingresado no corresponde a ninguna cuenta registrada.";
                return View(model);
            }

            // Simular activación si el usuario estaba inactivo
            if (!user.Activo)
            {
                user.Activo = true;
                await _usuarioRepository.UpdateAsync(user);
                _logger.LogInformation("Cuenta activada mediante DNI/CE: {Tipo} {Numero} para {Nombre}", model.TipoDocumento, model.NumeroDocumento, user.Nombre);
            }

            return RedirectToAction(nameof(Activated), new { nombre = user.Nombre });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado durante la activación de cuenta.");
            TempData["ActivateError"] = "Ocurrió un error inesperado. Intente nuevamente.";
            return View(model);
        }
    }

    // ── GET: /Account/Activated ────────────────────────────────────────
    [HttpGet]
    public IActionResult Activated(string nombre = "July")
    {
        ViewData["NombreUsuario"] = nombre;
        return View();
    }

    // ── GET: /Account/ForgotPassword ───────────────────────────────────
    [HttpGet]
    public IActionResult ForgotPassword()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Profile");

        return View(new ForgotPasswordViewModel());
    }

    // ── POST: /Account/ForgotPassword ──────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var user = await _usuarioRepository.GetByDocumentAsync(model.TipoDocumento, model.NumeroDocumento);
            if (user == null || !string.Equals(user.Email, model.Email, StringComparison.OrdinalIgnoreCase))
            {
                TempData["ForgotError"] = "Los datos ingresados no coinciden con nuestros registros o la cuenta no existe.";
                return View(model);
            }

            // Simular envío de correo de recuperación en logs
            _logger.LogWarning(
                "[SIMULACIÓN SMTP] Envío de recuperación de contraseña solicitado.\n" +
                "Destinatario: {Email} | Usuario: {Username} | Instrucciones de restablecimiento enviadas.",
                user.Email, user.NombreUsuario);

            ViewData["Email"] = user.Email;
            return View("ForgotPasswordConfirmation");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado durante la recuperación de contraseña.");
            TempData["ForgotError"] = "Ocurrió un error inesperado. Intente nuevamente.";
            return View(model);
        }
    }

    // ── GET: /Account/Login ────────────────────────────────────────────
    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Profile");

        if (TempData["ErrorType"]?.ToString() == "lockout")
        {
            ViewBag.IsBlocked = true;
            ViewBag.BlockMinutes = _configuration.GetValue<int>("Seguridad:MinutosBloqueo", 15);
        }

        return View(new LoginViewModel());
    }

    // ── POST: /Account/Login ───────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            // ── Validar credenciales y gestionar bloqueos en el Servicio ───────────
            var authResult = await _authService.ValidateCredentialsAsync(
                model.NombreUsuario, model.Password);

            if (!authResult.Success || authResult.Usuario is null)
            {
                TempData["LoginUsername"] = model.NombreUsuario;

                if (authResult.ErrorType == AuthErrorType.AccountLocked)
                {
                    TempData["LoginError"] = $"Su cuenta ha sido bloqueada temporalmente por {authResult.LockoutMinutesRemaining} minutos. Se ha enviado un correo de notificación.";
                    TempData["ErrorType"] = "lockout";
                }
                else
                {
                    var mensajesInlineFigma = _configuration.GetValue<bool>("Seguridad:MensajesInlineFigma", false);
                    if (mensajesInlineFigma)
                    {
                        if (authResult.ErrorType == AuthErrorType.UserNotFound)
                        {
                            TempData["LoginError"] = "Usuario incorrecto.";
                        }
                        else if (authResult.ErrorType == AuthErrorType.IncorrectPassword)
                        {
                            TempData["LoginError"] = "Contraseña incorrecta.";
                        }
                        else if (authResult.ErrorType == AuthErrorType.UserInactive)
                        {
                            TempData["LoginError"] = "El usuario se encuentra inactivo.";
                        }
                        else
                        {
                            TempData["LoginError"] = "Usuario o contraseña incorrectos. Intente nuevamente.";
                        }
                    }
                    else
                    {
                        // Mensaje unificado para evitar enumeración de usuarios (UserNotFound, IncorrectPassword, UserInactive)
                        TempData["LoginError"] = "Usuario o contraseña incorrectos. Intente nuevamente.";
                    }
                    TempData["ErrorType"] = "danger";
                }

                return RedirectToAction(nameof(Login));
            }

            var usuarioLogueado = authResult.Usuario;

            // ── Crear claims y autenticar ──────────────────────────
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, usuarioLogueado.Id.ToString()),
                new(ClaimTypes.Name, $"{usuarioLogueado.PrimerApellido} {usuarioLogueado.SegundoApellido}, {usuarioLogueado.Nombre}"),
                new(ClaimTypes.Email, usuarioLogueado.Email),
                new(ClaimTypes.Role, usuarioLogueado.Rol),
                new("NombreUsuario", usuarioLogueado.NombreUsuario),
                new("Nombre", usuarioLogueado.Nombre),
                new("PrimerApellido", usuarioLogueado.PrimerApellido),
                new("SegundoApellido", usuarioLogueado.SegundoApellido),
                new("AvatarUrl", usuarioLogueado.AvatarUrl ?? "/images/avatar-default.png")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var minutosSesion = _configuration.GetValue<int>("Seguridad:MinutosSesion", 30);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = false,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(minutosSesion)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authProperties);

            return RedirectToAction("Index", "Profile");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado durante el proceso de login en la presentación.");
            TempData["LoginError"] = "Ocurrió un error inesperado. Intente nuevamente más tarde.";
            TempData["ErrorType"] = "danger";
            return RedirectToAction(nameof(Login));
        }
    }

    // ── POST: /Account/ContactSupport ──────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ContactSupport(string username)
    {
        // Simular envío de reporte de soporte técnico
        _logger.LogWarning(
            "[SOPORTE TÉCNICO] Incidente de bloqueo reportado para el usuario: {Username}.\n" +
            "Se ha creado el ticket de atención #SPT-{Random} y se notificó al personal encargado.",
            username ?? "Desconocido", Random.Shared.Next(1000, 9999));

        TempData["SupportSuccess"] = "Se ha enviado el reporte al área de soporte técnico. Evaluaremos su caso a la brevedad.";
        return RedirectToAction(nameof(Login));
    }

    // ── POST: /Account/Logout ──────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }

    // ── POST: /Account/KeepAlive ──────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult KeepAlive()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return Json(new { success = true });
        }
        return Json(new { success = false });
    }
}
