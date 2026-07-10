namespace PruebaAspNet.Models;

/// <summary>
/// ViewModel para la página de errores.
/// </summary>
public class ErrorViewModel
{
    /// <summary>
    /// Identificador único de la solicitud para seguimiento.
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// Indica si se debe mostrar el RequestId al usuario.
    /// </summary>
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
