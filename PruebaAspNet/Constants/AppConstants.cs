namespace PruebaAspNet.Constants;

/// <summary>
/// Constantes globales de la aplicación.
/// Evita valores mágicos dispersos en el código.
/// </summary>
public static class AppConstants
{
    // ── Autenticación ─────────────────────────────────────────────────
    public const int MaxFailedAttempts = 5;
    public const int LockoutMinutes = 15;
    public const int SessionTimeoutMinutes = 30;

    // ── Rate Limiting ─────────────────────────────────────────────────
    public const int RateLimitPermitLimit = 10;
    public const int RateLimitWindowMinutes = 1;

    // ── BCrypt ────────────────────────────────────────────────────────
    public const int BcryptWorkFactor = 12;
}
