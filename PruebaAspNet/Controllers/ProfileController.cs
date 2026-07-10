using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PruebaAspNet.Repositories;
using PruebaAspNet.ViewModels;

namespace PruebaAspNet.Controllers;

/// <summary>
/// Controlador para la visualización del perfil de usuario.
/// Requiere autenticación para acceder.
/// </summary>
[Authorize]
public class ProfileController : Controller
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(IUsuarioRepository usuarioRepository, ILogger<ProfileController> logger)
    {
        _usuarioRepository = usuarioRepository;
        _logger = logger;
    }

    // ── GET: /Profile ──────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                _logger.LogWarning("No se pudo obtener el ID del usuario autenticado.");
                return RedirectToAction("Login", "Account");
            }

            var usuario = await _usuarioRepository.GetByIdAsync(userId);
            if (usuario is null)
            {
                _logger.LogWarning("Usuario con ID {UserId} no encontrado.", userId);
                return RedirectToAction("Login", "Account");
            }

            var iniciales = $"{usuario.Nombre[..1]}{usuario.PrimerApellido[..1]}".ToUpper();

            var profile = new ProfileViewModel
            {
                NombreCompleto = $"{usuario.PrimerApellido} {usuario.SegundoApellido}, {usuario.Nombre}",
                Rol = usuario.Rol,
                Estado = usuario.Activo ? "Activo" : "Inactivo",
                Entidad = usuario.Entidad ?? "Sin entidad asignada",
                Iniciales = iniciales,
                Nombres = usuario.Nombre,
                PrimerApellido = usuario.PrimerApellido,
                SegundoApellido = usuario.SegundoApellido,
                TipoDocumento = usuario.TipoDocumento ?? "DNI",
                NumeroDocumento = usuario.NombreUsuario,
                FechaNacimiento = usuario.FechaNacimiento?.ToString("dd/MM/yyyy") ?? "No especificada",
                Nacionalidad = usuario.Nacionalidad ?? "No especificada",
                Sexo = usuario.Sexo ?? "No especificado",
                CorreoPrincipal = usuario.Email,
                CorreoSecundario = null,
                TelefonoMovil = usuario.TelefonoMovil ?? "No registrado",
                TipoTelefonoSecundario = null,
                TelefonoSecundario = null,
                TipoContratacion = usuario.TipoContratacion ?? "No especificada",
                FechaContratacion = usuario.FechaContratacion?.ToString("dd/MM/yyyy") ?? "No especificada",
                TabActivo = "informacion-basica",
                AvatarUrl = usuario.AvatarUrl ?? "/images/avatar-default.png",
                NombreUsuario = usuario.NombreUsuario
            };

            return View(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar el perfil de usuario.");
            return RedirectToAction("Login", "Account");
        }
    }
}
