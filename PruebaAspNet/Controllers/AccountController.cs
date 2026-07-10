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
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        IAuthService authService,
        IEmailService emailService,
        IUsuarioRepository usuarioRepository,
        ILogger<AccountController> logger)
    {
        _authService = authService;
        _emailService = emailService;
        _usuarioRepository = usuarioRepository;
        _logger = logger;
    }

    // ── GET: /Account/Activated ────────────────────────────────────────
    [HttpGet]
    public IActionResult Activated(string nombre = "July")
    {
        ViewData["NombreUsuario"] = nombre;
        return View();
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
            ViewBag.BlockMinutes = AppConstants.LockoutMinutes;
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
                    // Mensaje unificado para evitar enumeración de usuarios (UserNotFound, IncorrectPassword, UserInactive)
                    TempData["LoginError"] = "Usuario o contraseña incorrectos. Intente nuevamente.";
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

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = false,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(AppConstants.SessionTimeoutMinutes)
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
