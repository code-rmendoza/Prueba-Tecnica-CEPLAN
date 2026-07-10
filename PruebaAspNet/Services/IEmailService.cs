namespace PruebaAspNet.Services;

/// <summary>
/// Contrato para el servicio de envío de correos electrónicos.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Envía un correo de notificación de cuenta bloqueada.
    /// </summary>
    Task SendAccountLockedEmailAsync(string toEmail, string nombre, string username, DateTime lockedUntil);
}
