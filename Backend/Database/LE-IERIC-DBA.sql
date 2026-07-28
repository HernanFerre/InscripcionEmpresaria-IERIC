/*
    Base de datos: LE-IERIC
    Destinatario: DBA / Administración de Base de Datos

    Alcance:
    - Crea la base [LE-IERIC] si no existe.
    - Crea las tablas, claves, relaciones e índices del modelo inicial.
    - Registra la migración de Entity Framework.
    - No crea logins, usuarios ni contraseñas.

    El script es idempotente respecto de la migración registrada.
*/

USE [master];
GO

IF DB_ID(N'LE-IERIC') IS NULL
BEGIN
    EXEC(N'CREATE DATABASE [LE-IERIC]');
END;
GO

USE [LE-IERIC];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'[dbo].[__EFMigrationsHistory]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory]
            PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT 1
    FROM [dbo].[__EFMigrationsHistory]
    WHERE [MigrationId] =
        N'20260728202448_AddQuizPersistence'
)
BEGIN
    CREATE TABLE [dbo].[Empresa] (
        [Id] uniqueidentifier NOT NULL,
        [RazonSocial] nvarchar(max) NULL,
        [Cuit] bigint NULL,
        [EsCooperativa] bit NOT NULL,
        [EstadoActivo] bit NOT NULL,
        [LegacyId] int NOT NULL,
        [Activo] bit NOT NULL,
        [FechaAlta] datetime2 NOT NULL,
        [UsuarioAlta] nvarchar(max) NULL,
        [FechaUpdate] datetime2 NOT NULL,
        [UsuarioUpdate] nvarchar(max) NULL,
        CONSTRAINT [PK_Empresa]
            PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM [dbo].[__EFMigrationsHistory]
    WHERE [MigrationId] =
        N'20260728202448_AddQuizPersistence'
)
BEGIN
    CREATE TABLE [dbo].[QuizSesion] (
        [Id] uniqueidentifier NOT NULL,
        [CuitEmpresa] char(11) NOT NULL,
        [Estado] int NOT NULL,
        [IntentosTotales] int NOT NULL,
        [IntentosRestantes] int NOT NULL,
        [FechaCreacionUtc] datetime2 NOT NULL,
        [FechaExpiracionUtc] datetime2 NOT NULL,
        [FechaFinalizacionUtc] datetime2 NULL,
        CONSTRAINT [PK_QuizSesion]
            PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM [dbo].[__EFMigrationsHistory]
    WHERE [MigrationId] =
        N'20260728202448_AddQuizPersistence'
)
BEGIN
    CREATE TABLE [dbo].[QuizCuilVinculado] (
        [QuizId] uniqueidentifier NOT NULL,
        [Cuil] char(11) NOT NULL,
        CONSTRAINT [PK_QuizCuilVinculado]
            PRIMARY KEY ([QuizId], [Cuil]),
        CONSTRAINT [FK_QuizCuilVinculado_QuizSesion_QuizId]
            FOREIGN KEY ([QuizId])
            REFERENCES [dbo].[QuizSesion] ([Id])
            ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM [dbo].[__EFMigrationsHistory]
    WHERE [MigrationId] =
        N'20260728202448_AddQuizPersistence'
)
BEGIN
    CREATE TABLE [dbo].[QuizDesafio] (
        [Id] uniqueidentifier NOT NULL,
        [QuizId] uniqueidentifier NOT NULL,
        [Numero] int NOT NULL,
        [Escenario] int NOT NULL,
        [EsActual] bit NOT NULL,
        [FechaCreacionUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_QuizDesafio]
            PRIMARY KEY ([Id]),
        CONSTRAINT [FK_QuizDesafio_QuizSesion_QuizId]
            FOREIGN KEY ([QuizId])
            REFERENCES [dbo].[QuizSesion] ([Id])
            ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM [dbo].[__EFMigrationsHistory]
    WHERE [MigrationId] =
        N'20260728202448_AddQuizPersistence'
)
BEGIN
    CREATE TABLE [dbo].[QuizOpcion] (
        [QuizDesafioId] uniqueidentifier NOT NULL,
        [Id] varchar(10) NOT NULL,
        [Cuil] char(11) NOT NULL,
        [EsVinculado] bit NOT NULL,
        [Orden] int NOT NULL,
        CONSTRAINT [PK_QuizOpcion]
            PRIMARY KEY ([QuizDesafioId], [Id]),
        CONSTRAINT [FK_QuizOpcion_QuizDesafio_QuizDesafioId]
            FOREIGN KEY ([QuizDesafioId])
            REFERENCES [dbo].[QuizDesafio] ([Id])
            ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM [dbo].[__EFMigrationsHistory]
    WHERE [MigrationId] =
        N'20260728202448_AddQuizPersistence'
)
BEGIN
    CREATE TABLE [dbo].[QuizRespuesta] (
        [Id] uniqueidentifier NOT NULL,
        [QuizDesafioId] uniqueidentifier NOT NULL,
        [OpcionesSeleccionadas] nvarchar(200) NOT NULL,
        [EsCorrecta] bit NOT NULL,
        [FechaRespuestaUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_QuizRespuesta]
            PRIMARY KEY ([Id]),
        CONSTRAINT [FK_QuizRespuesta_QuizDesafio_QuizDesafioId]
            FOREIGN KEY ([QuizDesafioId])
            REFERENCES [dbo].[QuizDesafio] ([Id])
            ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM [dbo].[__EFMigrationsHistory]
    WHERE [MigrationId] =
        N'20260728202448_AddQuizPersistence'
)
BEGIN
    EXEC(
        N'CREATE UNIQUE INDEX [UX_QuizDesafio_Quiz_Actual]
          ON [dbo].[QuizDesafio] ([QuizId])
          WHERE [EsActual] = 1'
    );
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM [dbo].[__EFMigrationsHistory]
    WHERE [MigrationId] =
        N'20260728202448_AddQuizPersistence'
)
BEGIN
    CREATE UNIQUE INDEX [UX_QuizDesafio_Quiz_Numero]
        ON [dbo].[QuizDesafio] ([QuizId], [Numero]);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM [dbo].[__EFMigrationsHistory]
    WHERE [MigrationId] =
        N'20260728202448_AddQuizPersistence'
)
BEGIN
    CREATE UNIQUE INDEX [UX_QuizOpcion_Desafio_Orden]
        ON [dbo].[QuizOpcion] ([QuizDesafioId], [Orden]);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM [dbo].[__EFMigrationsHistory]
    WHERE [MigrationId] =
        N'20260728202448_AddQuizPersistence'
)
BEGIN
    CREATE INDEX [IX_QuizRespuesta_Desafio_Fecha]
        ON [dbo].[QuizRespuesta] (
            [QuizDesafioId],
            [FechaRespuestaUtc]
        );
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM [dbo].[__EFMigrationsHistory]
    WHERE [MigrationId] =
        N'20260728202448_AddQuizPersistence'
)
BEGIN
    CREATE INDEX [IX_QuizSesion_CuitEmpresa]
        ON [dbo].[QuizSesion] ([CuitEmpresa]);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM [dbo].[__EFMigrationsHistory]
    WHERE [MigrationId] =
        N'20260728202448_AddQuizPersistence'
)
BEGIN
    CREATE INDEX [IX_QuizSesion_Estado]
        ON [dbo].[QuizSesion] ([Estado]);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM [dbo].[__EFMigrationsHistory]
    WHERE [MigrationId] =
        N'20260728202448_AddQuizPersistence'
)
BEGIN
    INSERT INTO [dbo].[__EFMigrationsHistory] (
        [MigrationId],
        [ProductVersion]
    )
    VALUES (
        N'20260728202448_AddQuizPersistence',
        N'7.0.3'
    );
END;
GO

COMMIT;
GO
