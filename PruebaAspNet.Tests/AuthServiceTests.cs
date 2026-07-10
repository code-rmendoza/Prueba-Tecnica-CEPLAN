using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using PruebaAspNet.Models;
using PruebaAspNet.Repositories;
using PruebaAspNet.Services;

namespace PruebaAspNet.Tests;

public class AuthServiceTests
{
    private readonly Mock<IUsuarioRepository> _mockRepo;
    private readonly Mock<IEmailService> _mockEmail;
    private readonly Mock<ILogger<AuthService>> _mockLogger;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _mockRepo = new Mock<IUsuarioRepository>();
        _mockEmail = new Mock<IEmailService>();
        _mockLogger = new Mock<ILogger<AuthService>>();
        
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

        _authService = new AuthService(_mockRepo.Object, _mockEmail.Object, configuration, _mockLogger.Object);
    }

    [Fact]
    public async Task ValidateCredentials_UserNotFound_ReturnsUserNotFound()
    {
        _mockRepo.Setup(r => r.GetByUsernameAsync("nonexistent"))
            .ReturnsAsync((Usuario?)null);

        var result = await _authService.ValidateCredentialsAsync("nonexistent", "password");

        result.Success.Should().BeFalse();
        result.ErrorType.Should().Be(AuthErrorType.UserNotFound);
    }

    [Fact]
    public async Task ValidateCredentials_InactiveUser_ReturnsUserInactive()
    {
        var user = new Usuario { NombreUsuario = "test", Activo = false, PasswordHash = "hash" };
        _mockRepo.Setup(r => r.GetByUsernameAsync("test"))
            .ReturnsAsync(user);

        var result = await _authService.ValidateCredentialsAsync("test", "password");

        result.Success.Should().BeFalse();
        result.ErrorType.Should().Be(AuthErrorType.UserInactive);
    }

    [Fact]
    public async Task ValidateCredentials_WrongPassword_ReturnsIncorrectPassword()
    {
        var user = new Usuario
        {
            NombreUsuario = "test",
            Activo = true,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword")
        };
        _mockRepo.Setup(r => r.GetByUsernameAsync("test"))
            .ReturnsAsync(user);

        var result = await _authService.ValidateCredentialsAsync("test", "wrongpassword");

        result.Success.Should().BeFalse();
        result.ErrorType.Should().Be(AuthErrorType.IncorrectPassword);
    }

    [Fact]
    public async Task ValidateCredentials_CorrectPassword_ReturnsSuccess()
    {
        var user = new Usuario
        {
            Id = 1,
            NombreUsuario = "test",
            Activo = true,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword"),
            Nombre = "Test",
            PrimerApellido = "User",
            Email = "test@test.com",
            Rol = "Admin"
        };
        _mockRepo.Setup(r => r.GetByUsernameAsync("test"))
            .ReturnsAsync(user);

        var result = await _authService.ValidateCredentialsAsync("test", "correctpassword");

        result.Success.Should().BeTrue();
        result.Usuario.Should().NotBeNull();
        result.Usuario!.NombreUsuario.Should().Be("test");
    }
}
