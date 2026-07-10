using Microsoft.EntityFrameworkCore;
using PruebaAspNet.Constants;
using PruebaAspNet.Models;

namespace PruebaAspNet.Data;

/// <summary>
/// Clase encargada de sembrar datos iniciales en la base de datos.
/// Se ejecuta al iniciar la aplicación para garantizar que existan
/// usuarios de prueba para el desarrollo.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        if (await context.Usuarios.AnyAsync())
            return;

        var usuarios = new List<Usuario>
        {
            new()
            {
                Nombre = "July Camila",
                PrimerApellido = "Mendoza",
                SegundoApellido = "Quispe",
                Email = "test@minsa.gob.pe",
                NombreUsuario = "07079879",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!", AppConstants.BcryptWorkFactor),
                Activo = true,
                Rol = "Administrador de Recursos",
                FechaCreacion = DateTime.UtcNow,
                TipoDocumento = "DNI",
                NumeroDocumento = "07079879",
                FechaNacimiento = new DateOnly(1990, 6, 15),
                Nacionalidad = "Peruana",
                Sexo = "Femenino",
                Entidad = "011 Ministerio de Salud",
                TelefonoMovil = "+51 999 888 777",
                TipoContratacion = "Nombrado",
                FechaContratacion = new DateOnly(2015, 3, 9),
                AvatarUrl = "/images/avatar-default.png"
            },
            new()
            {
                Nombre = "Adriana",
                PrimerApellido = "Osorio",
                SegundoApellido = "Montes",
                Email = "adriana.osorio@ceplan.gob.pe",
                NombreUsuario = "46844596",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("User123!", AppConstants.BcryptWorkFactor),
                Activo = true,
                Rol = "Operador",
                FechaCreacion = DateTime.UtcNow,
                TipoDocumento = "DNI",
                NumeroDocumento = "46844596",
                FechaNacimiento = new DateOnly(1992, 10, 28),
                Nacionalidad = "Peruana",
                Sexo = "Femenino",
                Entidad = "Centro Nacional de Planeamiento Estratégico",
                TelefonoMovil = "+51 999 123 456",
                TipoContratacion = "CAS",
                FechaContratacion = new DateOnly(2020, 1, 15),
                AvatarUrl = "/images/avatar-default.png"
            }
        };

        await context.Usuarios.AddRangeAsync(usuarios);
        await context.SaveChangesAsync();
    }
}
