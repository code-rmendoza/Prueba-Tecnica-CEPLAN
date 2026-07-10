using PruebaAspNet.Constants;
using PruebaAspNet.Models;
using PruebaAspNet.Repositories;

namespace PruebaAspNet.Services;

/// <summary>
/// Servicio de autenticación que valida credenciales contra la base de datos.
/// Utiliza BCrypt para la verificación segura de contraseñas.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUsuarioRepository usuarioRepository, IEmailService emailService, ILogger<AuthService> logger)
    {
        _usuarioRepository = usuarioRepository;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<AuthResult> ValidateCredentialsAsync(string username, string password)
    {
        var maskedUser = MaskUsername(username);
        try
        {
            var usuario = await _usuarioRepository.GetByUsernameAsync(username);

            if (usuario is null)
            {
                _logger.LogWarning("Intento de inicio de sesión con usuario inexistente: {Username}", maskedUser);
                return new AuthResult { ErrorType = AuthErrorType.UserNotFound };
            }

            if (!usuario.Activo)
            {
                _logger.LogWarning("Intento de inicio de sesión con usuario inactivo: {Username}", maskedUser);
                return new AuthResult { ErrorType = AuthErrorType.UserInactive };
            }

            // ── Verificar si el usuario está bloqueado ──────────────
            if (usuario.BloqueadoHasta.HasValue)
            {
                if (DateTime.UtcNow < usuario.BloqueadoHasta.Value)
                {
                    var minutosRestantes = (int)(usuario.BloqueadoHasta.Value - DateTime.UtcNow).TotalMinutes + 1;
                    _logger.LogWarning("Intento de acceso a cuenta bloqueada: {Username}. Bloqueo expira en {Minutes} min.", maskedUser, minutosRestantes);
                    return new AuthResult 
                    { 
                        ErrorType = AuthErrorType.AccountLocked, 
                        LockoutMinutesRemaining = minutosRestantes 
                    };
                }
                else
                {
                    // Bloqueo expiró: resetear campos
                    usuario.IntentosFallidos = 0;
                    usuario.BloqueadoHasta = null;
                    await _usuarioRepository.UpdateAsync(usuario);
                    _logger.LogInformation("Bloqueo de cuenta expirado para el usuario: {Username}. Reiniciando intentos.", maskedUser);
                }
            }

            // ── Validar contraseña ───────────────────────────────
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash);

            if (!isPasswordValid)
            {
                usuario.IntentosFallidos++;
                _logger.LogWarning("Contraseña incorrecta para el usuario: {Username}. Intentos fallidos: {Attempts}", maskedUser, usuario.IntentosFallidos);

                if (usuario.IntentosFallidos >= AppConstants.MaxFailedAttempts)
                {
                    var lockTime = DateTime.UtcNow.AddMinutes(AppConstants.LockoutMinutes);
                    usuario.IntentosFallidos = 0; // Se resetea al bloquear
                    usuario.BloqueadoHasta = lockTime;
                    await _usuarioRepository.UpdateAsync(usuario);

                    _logger.LogWarning("Cuenta {Username} bloqueada temporalmente por {Minutes} minutos tras {Max} intentos fallidos.",
                        maskedUser, AppConstants.LockoutMinutes, AppConstants.MaxFailedAttempts);

                    try
                    {
                        await _emailService.SendAccountLockedEmailAsync(
                            usuario.Email, usuario.Nombre, usuario.NombreUsuario, lockTime);
                    }
                    catch (Exception emailEx)
                    {
                        _logger.LogWarning(emailEx, "No se pudo enviar correo de bloqueo al usuario: {Username}", maskedUser);
                    }

                    return new AuthResult
                    {
                        ErrorType = AuthErrorType.AccountLocked,
                        LockoutMinutesRemaining = AppConstants.LockoutMinutes
                    };
                }

                await _usuarioRepository.UpdateAsync(usuario);

                return new AuthResult 
                { 
                    ErrorType = AuthErrorType.IncorrectPassword, 
                    RemainingAttempts = AppConstants.MaxFailedAttempts - usuario.IntentosFallidos 
                };
            }

            // ── Login exitoso ──────────────────────────────────────
            if (usuario.IntentosFallidos > 0 || usuario.BloqueadoHasta.HasValue)
            {
                usuario.IntentosFallidos = 0;
                usuario.BloqueadoHasta = null;
                await _usuarioRepository.UpdateAsync(usuario);
            }

            _logger.LogInformation("Inicio de sesión exitoso para el usuario: {Username}", maskedUser);
            return new AuthResult { Usuario = usuario, ErrorType = AuthErrorType.None };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado durante la autenticación del usuario: {Username}", maskedUser);
            return new AuthResult { ErrorType = AuthErrorType.UnexpectedError };
        }
    }

    private string MaskUsername(string username)
    {
        if (string.IsNullOrEmpty(username))
            return string.Empty;
        if (username.Length < 4)
            return "***";
        return $"{username[..2]}***{username[^2..]}";
    }
}
