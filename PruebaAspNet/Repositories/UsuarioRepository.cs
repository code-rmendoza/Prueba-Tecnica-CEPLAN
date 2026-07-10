using Microsoft.EntityFrameworkCore;
using PruebaAspNet.Data;
using PruebaAspNet.Models;

namespace PruebaAspNet.Repositories;

/// <summary>
/// Implementación del repositorio de usuarios.
/// Accede a la base de datos a través de Entity Framework Core.
/// </summary>
public class UsuarioRepository : IUsuarioRepository
{
    private readonly ApplicationDbContext _context;

    public UsuarioRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> GetByUsernameAsync(string username)
    {
        return await _context.Usuarios
            .FirstOrDefaultAsync(u => u.NombreUsuario == username);
    }

    public async Task<Usuario?> GetByIdAsync(int id)
    {
        return await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task UpdateAsync(Usuario usuario)
    {
        _context.Usuarios.Update(usuario);
        await _context.SaveChangesAsync();
    }
}
