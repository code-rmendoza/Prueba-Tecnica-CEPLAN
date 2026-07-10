using System.ComponentModel.DataAnnotations;

namespace PruebaAspNet.ViewModels;

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "El tipo de documento es requerido.")]
    public string TipoDocumento { get; set; } = "DNI";

    [Required(ErrorMessage = "El número de documento es requerido.")]
    [StringLength(20, MinimumLength = 8, ErrorMessage = "El número de documento debe tener entre 8 y 20 caracteres.")]
    public string NumeroDocumento { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es requerido.")]
    [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
    public string Email { get; set; } = string.Empty;
}
