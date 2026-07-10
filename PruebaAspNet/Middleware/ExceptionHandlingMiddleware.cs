using System.Net;

namespace PruebaAspNet.Middleware;

/// <summary>
/// Middleware global de manejo de excepciones.
/// Captura errores no controlados y retorna una respuesta JSON o redirige a Error.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción no controlada: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var isAjax = context.Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                     context.Request.Headers["Accept"].ToString().Contains("application/json");

        if (isAjax)
        {
            context.Response.ContentType = "application/json; charset=utf-8";
            var jsonResponse = System.Text.Json.JsonSerializer.Serialize(new
            {
                success = false,
                error = "Ocurrió un error inesperado en el servidor."
            });
            await context.Response.WriteAsync(jsonResponse);
        }
        else
        {
            context.Response.ContentType = "text/html; charset=utf-8";
            var html = $@"<!DOCTYPE html>
<html><head><title>Error del Servidor</title></head>
<body style='font-family:Arial,sans-serif;text-align:center;padding:60px;'>
  <h1 style='color:#9A1413;'>Error del Servidor</h1>
  <p>Ocurrió un error inesperado. Por favor, intente nuevamente más tarde.</p>
  <a href='/Account/Login' style='color:#9A1413;'>Volver al inicio</a>
</body></html>";

            await context.Response.WriteAsync(html);
        }
    }
}
