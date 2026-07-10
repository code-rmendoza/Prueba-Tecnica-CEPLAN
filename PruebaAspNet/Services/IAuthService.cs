using PruebaAspNet.Models;

namespace PruebaAspNet.Services;

/// <summary>
/// Tipos de errores posibles durante la autenticación.
/// </summary>
public enum AuthErrorType
{
    None,
    UserNotFound,
    UserInactive,
    IncorrectPassword,
    AccountLocked,
    UnexpectedError
}

/// <summary>
/// Resultado detallado de la autenticación.
/// </summary>
public class AuthResult
{
    public bool Success => ErrorType == AuthErrorType.None && Usuario != null;
    public Usuario? Usuario { get; set; }
    public AuthErrorType ErrorType { get; set; } = AuthErrorType.None;
    public int RemainingAttempts { get; set; }
    public int LockoutMinutesRemaining { get; set; }
}

/// <summary>
/// Contrato para el servicio de autenticación.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Valida las credenciales del usuario, gestiona intentos fallidos, bloqueos de cuenta y envía notificaciones.
    /// </summary>
    Task<AuthResult> ValidateCredentialsAsync(string username, string password);
}
