using System.ComponentModel.DataAnnotations;

namespace PruebaAspNet.ViewModels;

/// <summary>
/// ViewModel para el formulario de inicio de sesión.
/// Contiene las validaciones del lado del servidor mediante DataAnnotations.
/// </summary>
public class LoginViewModel
{
    [Required(ErrorMessage = "El usuario es obligatorio.")]
    [StringLength(50, ErrorMessage = "El usuario no puede exceder los 50 caracteres.")]
    [Display(Name = "Usuario")]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de documento seleccionado: "DNI" o "CE".
    /// </summary>
    [Display(Name = "Tipo de Documento")]
    public string TipoDocumento { get; set; } = "DNI";
}
