using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using PruebaAspNet.Models;

namespace PruebaAspNet.Services;

/// <summary>
/// Servicio de envío de correos electrónicos.
/// En producción se configuraría SMTP real. Actualmente envía el correo
/// si se configura SmtpSettings en appsettings.json, o solo registra el intento.
/// </summary>
public class EmailService : IEmailService
{
    private readonly SmtpSettings _smtpSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<SmtpSettings> smtpSettings, ILogger<EmailService> logger)
    {
        _smtpSettings = smtpSettings.Value;
        _logger = logger;
    }

    public async Task SendAccountLockedEmailAsync(string toEmail, string nombre, string username, DateTime lockedUntil)
    {
        var subject = "Notificación de cuenta bloqueada - CEPLAN";
        var timeRemaining = (lockedUntil - DateTime.UtcNow).Minutes + 1;

        var body = $@"
<!DOCTYPE html>
<html>
<head><meta charset='utf-8'></head>
<body style='font-family: Arial, sans-serif; background-color: #f8f8f8; margin: 0; padding: 20px;'>
  <div style='max-width: 500px; margin: 0 auto; background: white; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.1);'>
    <div style='background-color: #9A1413; padding: 20px; text-align: center;'>
      <h1 style='color: white; margin: 0; font-size: 18px;'>CEPLAN - Sistema de Gestión</h1>
    </div>
    <div style='padding: 30px;'>
      <h2 style='color: #181A1C; font-size: 20px; margin-top: 0;'>Cuenta bloqueada temporalmente</h2>
      <p style='color: #515D68; font-size: 14px; line-height: 22px;'>
        Estimado(a) <strong>{nombre}</strong>,
      </p>
      <p style='color: #515D68; font-size: 14px; line-height: 22px;'>
        Su cuenta <strong>{username}</strong> ha sido bloqueada temporalmente debido a múltiples intentos de acceso fallidos.
      </p>
      <div style='background-color: #FDEAEA; border-left: 4px solid #D20000; padding: 12px 16px; margin: 20px 0; border-radius: 4px;'>
        <p style='color: #D20000; font-size: 14px; margin: 0;'>
          <strong>Duración del bloqueo:</strong> {timeRemaining} minutos<br>
          <strong>Desbloqueo estimado:</strong> {lockedUntil:dd/MM/yyyy HH:mm} UTC
        </p>
      </div>
      <p style='color: #515D68; font-size: 14px; line-height: 22px;'>
        Si usted no realizó estos intentos de acceso, contacte al administrador del sistema inmediatamente.
      </p>
      <p style='color: #515D68; font-size: 14px; line-height: 22px;'>
        Este es un correo automático, por favor no responda a este mensaje.
      </p>
    </div>
    <div style='background-color: #f8f8f8; padding: 16px; text-align: center; border-top: 1px solid #e0e0e0;'>
      <p style='color: #747D86; font-size: 12px; margin: 0;'>
        Centro Nacional de Planeamiento Estratégico - CEPLAN
      </p>
    </div>
  </div>
</body>
</html>";

        if (!string.IsNullOrEmpty(_smtpSettings.Host) && !string.IsNullOrEmpty(_smtpSettings.Username))
        {
            try
            {
                using var client = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
                {
                    Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_smtpSettings.FromEmail, _smtpSettings.FromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Correo de bloqueo enviado a {Email} para el usuario {Username}", toEmail, username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar correo de bloqueo a {Email}", toEmail);
            }
        }
        else
        {
            _logger.LogWarning(
                "CONFIGURACIÓN SMTP NO ENCONTRADA. Correo de bloqueo no enviado.\n" +
                "Para habilitar el envío, configure SmtpSettings en appsettings.json.\n" +
                "Destinatario: {Email} | Usuario: {Username} | Desbloqueo: {LockedUntil:dd/MM/yyyy HH:mm} UTC",
                toEmail, username, lockedUntil);
        }
    }
}
