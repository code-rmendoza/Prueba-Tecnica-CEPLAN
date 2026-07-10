namespace PruebaAspNet.Models;

/// <summary>
/// Modelo de configuración SMTP para envío de correos electrónicos.
/// Se enlaza desde appsettings.json bajo la sección "SmtpSettings".
/// </summary>
public class SmtpSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = "no-reply@ceplan.gob.pe";
    public string FromName { get; set; } = "CEPLAN Sistema de Gestión";
}
