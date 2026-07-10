using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using PruebaAspNet.Constants;
using PruebaAspNet.Controllers;
using PruebaAspNet.Models;
using PruebaAspNet.Repositories;
using PruebaAspNet.Services;
using PruebaAspNet.ViewModels;

namespace PruebaAspNet.Tests;

public class AccountControllerTests
{
    private readonly Mock<IAuthService> _mockAuth;
    private readonly Mock<IEmailService> _mockEmail;
    private readonly Mock<IUsuarioRepository> _mockRepo;
    private readonly Mock<ILogger<AccountController>> _mockLogger;
    private readonly AccountController _controller;

    public AccountControllerTests()
    {
        _mockAuth = new Mock<IAuthService>();
        _mockEmail = new Mock<IEmailService>();
        _mockRepo = new Mock<IUsuarioRepository>();
        _mockLogger = new Mock<ILogger<AccountController>>();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"Seguridad:MaxIntentosFallidos", "5"},
                {"Seguridad:MinutosBloqueo", "15"},
                {"Seguridad:MinutosSesion", "30"},
                {"Seguridad:SegundosAvisoExpiracion", "30"},
                {"Seguridad:MensajesInlineFigma", "false"}
            })
            .Build();

        _controller = new AccountController(
            _mockAuth.Object, _mockEmail.Object, _mockRepo.Object, configuration, _mockLogger.Object);

        // ── Configurar HttpContext mockeado ──
        var httpContext = new DefaultHttpContext();

        // Mockear IAuthenticationService para evitar errores en SignIn/SignOut
        var authServiceMock = new Mock<IAuthenticationService>();
        authServiceMock.Setup(a => a.SignInAsync(
            It.IsAny<HttpContext>(),
            It.IsAny<string>(),
            It.IsAny<ClaimsPrincipal>(),
            It.IsAny<AuthenticationProperties>()))
            .Returns(Task.CompletedTask);

        authServiceMock.Setup(a => a.SignOutAsync(
            It.IsAny<HttpContext>(),
            It.IsAny<string>(),
            It.IsAny<AuthenticationProperties>()))
            .Returns(Task.CompletedTask);

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(s => s.GetService(typeof(IAuthenticationService)))
            .Returns(authServiceMock.Object);

        httpContext.RequestServices = serviceProviderMock.Object;

        // Configurar TempData
        var tempDataProvider = new Mock<ITempDataProvider>();
        var tempData = new TempDataDictionary(httpContext, tempDataProvider.Object);
        _controller.TempData = tempData;

        // Configurar ControllerContext
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Mockear IUrlHelper para evitar dependencias de IUrlHelperFactory al redireccionar
        var mockUrlHelper = new Mock<IUrlHelper>();
        _controller.Url = mockUrlHelper.Object;
    }

    [Fact]
    public void Login_GET_ReturnsView()
    {
        var result = _controller.Login();

        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult!.Model.Should().BeOfType<LoginViewModel>();
    }

    [Fact]
    public void Activated_ReturnsView()
    {
        var result = _controller.Activated("TestUser");

        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult!.ViewData["NombreUsuario"].Should().Be("TestUser");
    }

    [Fact]
    public async Task Login_POST_InvalidModel_ReturnsView()
    {
        _controller.ModelState.AddModelError("Error", "Invalid");

        var result = await _controller.Login(new LoginViewModel());

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Login_POST_ValidCredentials_RedirectsToProfile()
    {
        var user = new Usuario
        {
            Id = 1,
            Nombre = "Test",
            PrimerApellido = "User",
            NombreUsuario = "07079879",
            Email = "test@test.com",
            Rol = "Admin",
            Activo = true
        };
        _mockAuth.Setup(a => a.ValidateCredentialsAsync("07079879", "Admin123!"))
            .ReturnsAsync(new AuthResult { Usuario = user, ErrorType = AuthErrorType.None });

        var result = await _controller.Login(new LoginViewModel
        {
            NombreUsuario = "07079879",
            Password = "Admin123!"
        });

        result.Should().BeOfType<RedirectToActionResult>();
        var redirect = result as RedirectToActionResult;
        redirect!.ActionName.Should().Be("Index");
        redirect.ControllerName.Should().Be("Profile");
    }

    [Fact]
    public async Task Login_POST_WrongPassword_RedirectsToLoginAndSetsError()
    {
        var user = new Usuario
        {
            Id = 1,
            NombreUsuario = "07079879",
            Activo = true
        };
        _mockAuth.Setup(a => a.ValidateCredentialsAsync("07079879", "wrong"))
            .ReturnsAsync(new AuthResult { ErrorType = AuthErrorType.IncorrectPassword });

        var result = await _controller.Login(new LoginViewModel
        {
            NombreUsuario = "07079879",
            Password = "wrong"
        });

        result.Should().BeOfType<RedirectToActionResult>();
        var redirect = result as RedirectToActionResult;
        redirect!.ActionName.Should().Be("Login");
        
        _controller.TempData["LoginError"].Should().Be("Usuario o contraseña incorrectos. Intente nuevamente.");
        _controller.TempData["ErrorType"].Should().Be("danger");
    }

    [Fact]
    public async Task Logout_ReturnsRedirectToLogin()
    {
        var result = await _controller.Logout();

        result.Should().BeOfType<RedirectToActionResult>();
        var redirect = result as RedirectToActionResult;
        redirect!.ActionName.Should().Be("Login");
    }

    [Fact]
    public void Activate_GET_ReturnsView()
    {
        var result = _controller.Activate();

        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult!.Model.Should().BeOfType<ActivateViewModel>();
    }

    [Fact]
    public async Task Activate_POST_NonExistentUser_ReturnsViewWithErrorMessage()
    {
        _mockRepo.Setup(r => r.GetByDocumentAsync("DNI", "12345678"))
            .ReturnsAsync((Usuario?)null);

        var result = await _controller.Activate(new ActivateViewModel
        {
            TipoDocumento = "DNI",
            NumeroDocumento = "12345678"
        });

        result.Should().BeOfType<ViewResult>();
        _controller.TempData["ActivateError"].Should().Be("El número de documento ingresado no corresponde a ninguna cuenta registrada.");
    }

    [Fact]
    public async Task Activate_POST_ExistingUser_RedirectsToActivated()
    {
        var user = new Usuario { Nombre = "July", Activo = false };
        _mockRepo.Setup(r => r.GetByDocumentAsync("DNI", "07079879"))
            .ReturnsAsync(user);

        var result = await _controller.Activate(new ActivateViewModel
        {
            TipoDocumento = "DNI",
            NumeroDocumento = "07079879"
        });

        result.Should().BeOfType<RedirectToActionResult>();
        var redirect = result as RedirectToActionResult;
        redirect!.ActionName.Should().Be("Activated");
        redirect.RouteValues!["nombre"].Should().Be("July");
    }

    [Fact]
    public void ForgotPassword_GET_ReturnsView()
    {
        var result = _controller.ForgotPassword();

        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult!.Model.Should().BeOfType<ForgotPasswordViewModel>();
    }

    [Fact]
    public async Task ForgotPassword_POST_MatchingUserAndEmail_ReturnsForgotPasswordConfirmation()
    {
        var user = new Usuario { NombreUsuario = "07079879", Email = "test@minsa.gob.pe" };
        _mockRepo.Setup(r => r.GetByDocumentAsync("DNI", "07079879"))
            .ReturnsAsync(user);

        var result = await _controller.ForgotPassword(new ForgotPasswordViewModel
        {
            TipoDocumento = "DNI",
            NumeroDocumento = "07079879",
            Email = "test@minsa.gob.pe"
        });

        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult!.ViewName.Should().Be("ForgotPasswordConfirmation");
    }
}
