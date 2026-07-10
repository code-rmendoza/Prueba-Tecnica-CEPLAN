using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PruebaAspNet.Models;

/// <summary>
/// Entidad que representa un usuario del sistema.
/// Mapeada directamente a la tabla [Usuarios] en SQL Server.
/// </summary>
[Table("Usuarios")]
public class Usuario
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string PrimerApellido { get; set; } = string.Empty;

    [MaxLength(100)]
    public string SegundoApellido { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string PasswordHash { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;

    [MaxLength(100)]
    public string Rol { get; set; } = string.Empty;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    // ── Seguridad: Brute-force tracking ──────────────────────────────
    /// <summary>
    /// Número de intentos fallidos consecutivos de login.
    /// </summary>
    public int IntentosFallidos { get; set; } = 0;

    /// <summary>
    /// Fecha y hora UTC hasta la cual el usuario está bloqueado.
    /// Null = no bloqueado.
    /// </summary>
    public DateTime? BloqueadoHasta { get; set; }

    // ── Perfil extendido ────────────────────────────────────────────
    [MaxLength(50)]
    public string TipoDocumento { get; set; } = "DNI";

    [MaxLength(20)]
    public string? NumeroDocumento { get; set; }

    public DateOnly? FechaNacimiento { get; set; }

    [MaxLength(50)]
    public string? Nacionalidad { get; set; }

    [MaxLength(20)]
    public string? Sexo { get; set; }

    [MaxLength(50)]
    public string? Entidad { get; set; }

    [MaxLength(20)]
    public string? TelefonoMovil { get; set; }

    [MaxLength(20)]
    public string? TipoContratacion { get; set; }

    public DateOnly? FechaContratacion { get; set; }

    [MaxLength(200)]
    public string? AvatarUrl { get; set; }
}
