using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PruebaAspNet.Models;

namespace PruebaAspNet.Controllers;

/// <summary>
/// Controlador para manejar errores y páginas generales.
/// </summary>
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Muestra la página de error genérica.
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        _logger.LogWarning("Error solicitado con RequestId: {RequestId}", requestId);

        return View(new ErrorViewModel
        {
            RequestId = requestId
        });
    }
}
