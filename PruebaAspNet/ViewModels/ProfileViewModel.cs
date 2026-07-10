namespace PruebaAspNet.ViewModels;

/// <summary>
/// ViewModel para la vista de perfil de usuario.
/// Contiene toda la información personal, de contacto y de contratación
/// que se muestra en el formulario de solo lectura.
/// </summary>
public class ProfileViewModel
{
    // ── Información de cabecera ────────────────────────────────────────
    public string NombreCompleto { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string Estado { get; set; } = "Activo";
    public string Entidad { get; set; } = string.Empty;
    public string Iniciales { get; set; } = string.Empty;

    // ── Información básica ─────────────────────────────────────────────
    public string Nombres { get; set; } = string.Empty;
    public string PrimerApellido { get; set; } = string.Empty;
    public string SegundoApellido { get; set; } = string.Empty;
    public string TipoDocumento { get; set; } = string.Empty;
    public string NumeroDocumento { get; set; } = string.Empty;
    public string FechaNacimiento { get; set; } = string.Empty;
    public string Nacionalidad { get; set; } = string.Empty;
    public string Sexo { get; set; } = string.Empty;

    // ── Contacto ───────────────────────────────────────────────────────
    public string CorreoPrincipal { get; set; } = string.Empty;
    public string? CorreoSecundario { get; set; }
    public string TelefonoMovil { get; set; } = string.Empty;
    public string? TipoTelefonoSecundario { get; set; }
    public string? TelefonoSecundario { get; set; }

    // ── Contratación ───────────────────────────────────────────────────
    public string TipoContratacion { get; set; } = string.Empty;
    public string FechaContratacion { get; set; } = string.Empty;

    // ── Tab activo ─────────────────────────────────────────────────────
    public string TabActivo { get; set; } = "informacion-basica";

    // ── Propiedades adicionales de fidelidad con Figma ──────────────────
    public string AvatarUrl { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
}

