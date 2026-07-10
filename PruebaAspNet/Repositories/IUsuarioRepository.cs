using PruebaAspNet.Models;

namespace PruebaAspNet.Repositories;

/// <summary>
/// Contrato para el acceso a datos de usuarios.
/// Permite desacoplar la lógica de negocio del acceso a datos.
/// </summary>
public interface IUsuarioRepository
{
    /// <summary>
    /// Obtiene un usuario por su nombre de usuario.
    /// </summary>
    Task<Usuario?> GetByUsernameAsync(string username);

    /// <summary>
    /// Obtiene un usuario por su Id.
    /// </summary>
    Task<Usuario?> GetByIdAsync(int id);

    /// <summary>
    /// Actualiza un usuario en la base de datos.
    /// </summary>
    Task UpdateAsync(Usuario usuario);
}
