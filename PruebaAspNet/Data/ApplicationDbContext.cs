using Microsoft.EntityFrameworkCore;
using PruebaAspNet.Models;

namespace PruebaAspNet.Data;

/// <summary>
/// Contexto de Entity Framework Core.
/// Administra la conexión a la base de datos y el mapeo de entidades.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("Usuarios");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.NombreUsuario).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();

            entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PrimerApellido).HasMaxLength(100).IsRequired();
            entity.Property(e => e.SegundoApellido).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(200).IsRequired();
            entity.Property(e => e.NombreUsuario).HasMaxLength(50).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Rol).HasMaxLength(100);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("GETUTCDATE()");
        });
    }
}
