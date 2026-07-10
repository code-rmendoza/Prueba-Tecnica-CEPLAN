/* ══════════════════════════════════════════════════════════════════════
   Base de Datos: PruebaAspNet
   Script de creación de la base de datos, tablas y datos iniciales
   ══════════════════════════════════════════════════════════════════════ */

-- ── Crear base de datos ────────────────────────────────────────────────
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'PruebaAspNet')
BEGIN
    CREATE DATABASE [PruebaAspNet];
END
GO

USE [PruebaAspNet];
GO

-- ── Crear tabla Usuarios ───────────────────────────────────────────────
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'Usuarios')
BEGIN
    CREATE TABLE [dbo].[Usuarios] (
        [Id]              INT             IDENTITY(1,1) NOT NULL,
        [Nombre]          NVARCHAR(100)   NOT NULL,
        [PrimerApellido]  NVARCHAR(100)   NOT NULL,
        [SegundoApellido] NVARCHAR(100)   NULL DEFAULT '',
        [Email]           NVARCHAR(200)   NOT NULL,
        [NombreUsuario]   NVARCHAR(50)    NOT NULL,
        [PasswordHash]    NVARCHAR(200)   NOT NULL,
        [Activo]          BIT             NOT NULL DEFAULT 1,
        [Rol]             NVARCHAR(100)   NULL DEFAULT '',
        [FechaCreacion]   DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
        [IntentosFallidos]    INT          NOT NULL DEFAULT 0,
        [BloqueadoHasta]      DATETIME2    NULL,
        [TipoDocumento]       NVARCHAR(50)  NULL DEFAULT 'DNI',
        [NumeroDocumento]     NVARCHAR(20)  NULL,
        [FechaNacimiento]     DATE          NULL,
        [Nacionalidad]        NVARCHAR(50)  NULL,
        [Sexo]                NVARCHAR(20)  NULL,
        [Entidad]             NVARCHAR(200) NULL,
        [TelefonoMovil]       NVARCHAR(20)  NULL,
        [TipoContratacion]    NVARCHAR(20)  NULL,
        [FechaContratacion]   DATE          NULL,
        [AvatarUrl]           NVARCHAR(200) NULL,

        CONSTRAINT [PK_Usuarios] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UQ_Usuarios_NombreUsuario] UNIQUE ([NombreUsuario]),
        CONSTRAINT [UQ_Usuarios_Email] UNIQUE ([Email])
    );
END
GO

-- ── Insertar usuario de prueba: July Camila Mendoza Quispe ─────────────
-- Contraseña: Admin123!
-- Nombre de usuario: DNI (07079879)
-- Hash generado con BCrypt (cost factor 11)
IF NOT EXISTS (SELECT 1 FROM [dbo].[Usuarios] WHERE [NombreUsuario] = N'07079879')
BEGIN
    INSERT INTO [dbo].[Usuarios] (
        [Nombre],
        [PrimerApellido],
        [SegundoApellido],
        [Email],
        [NombreUsuario],
        [PasswordHash],
        [Activo],
        [Rol],
        [FechaCreacion],
        [TipoDocumento],
        [NumeroDocumento],
        [FechaNacimiento],
        [Nacionalidad],
        [Sexo],
        [Entidad],
        [TelefonoMovil],
        [TipoContratacion],
        [FechaContratacion],
        [AvatarUrl]
    )
    VALUES (
        N'July Camila',
        N'Mendoza',
        N'Quispe',
        N'test@minsa.gob.pe',
        N'07079879',
        N'$2a$12$5.WIuLuXPxaGog4766xr9OOao1Q05hS3xdh0a2mNNOSetfZHWWh5q',
        1,
        N'Administrador de Recursos',
        GETUTCDATE(),
        N'DNI',
        N'07079879',
        '1990-06-15',
        N'Peruana',
        N'Femenino',
        N'011 Ministerio de Salud',
        N'+51 999 888 777',
        N'Nombrado',
        '2015-03-09',
        N'/images/avatar-default.png'
    );
END
GO

-- ── Insertar usuario de prueba: Adriana Osorio Montes ──────────────────
-- Contraseña: User123!
-- Nombre de usuario: DNI (46844596)
IF NOT EXISTS (SELECT 1 FROM [dbo].[Usuarios] WHERE [NombreUsuario] = N'46844596')
BEGIN
    INSERT INTO [dbo].[Usuarios] (
        [Nombre],
        [PrimerApellido],
        [SegundoApellido],
        [Email],
        [NombreUsuario],
        [PasswordHash],
        [Activo],
        [Rol],
        [FechaCreacion],
        [TipoDocumento],
        [NumeroDocumento],
        [FechaNacimiento],
        [Nacionalidad],
        [Sexo],
        [Entidad],
        [TelefonoMovil],
        [TipoContratacion],
        [FechaContratacion],
        [AvatarUrl]
    )
    VALUES (
        N'Adriana',
        N'Osorio',
        N'Montes',
        N'adriana.osorio@ceplan.gob.pe',
        N'46844596',
        N'$2a$12$iS/4k8C/4zHaw/2dMuvC/./wWaRhg6KgFCIlSkz2rhLTngiLvRnuK',
        1,
        N'Operador',
        GETUTCDATE(),
        N'DNI',
        N'46844596',
        '1992-10-28',
        N'Peruana',
        N'Femenino',
        N'Centro Nacional de Planeamiento Estratégico',
        N'+51 999 123 456',
        N'CAS',
        '2020-01-15',
        N'/images/avatar-default.png'
    );
END
GO

PRINT 'Base de datos PruebaAspNet creada exitosamente.';
PRINT 'Usuarios insertados:';
PRINT '  - 07079879 / Admin123! (July Camila Mendoza Quispe)';
PRINT '  - 46844596 / User123! (Adriana Osorio Montes)';
GO
