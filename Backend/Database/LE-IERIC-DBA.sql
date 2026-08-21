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
        N'20260821120000_RebuildQuizPersistence'
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

    CREATE TABLE [dbo].[QuizSesion] (
        [Id] bigint IDENTITY(1,1) NOT NULL,
        [CuitEmpresa] bigint NOT NULL,
        [UsuarioId] nvarchar(200) NOT NULL,
        [Estado] tinyint NOT NULL,
        [IntentosTotales] tinyint NOT NULL,
        [FechaCreacion] datetime NOT NULL,
        [FechaExpiracion] datetime NOT NULL,
        [FechaFinalizacion] datetime NULL,
        [BloqueadoHasta] datetime NULL,
        CONSTRAINT [PK_QuizSesion]
            PRIMARY KEY ([Id]),
        CONSTRAINT [CK_QuizSesion_Estado]
            CHECK ([Estado] BETWEEN 1 AND 4),
        CONSTRAINT [CK_QuizSesion_IntentosTotales]
            CHECK ([IntentosTotales] > 0)
    );

    CREATE TABLE [dbo].[QuizCuilVinculado] (
        [QuizSesionId] bigint NOT NULL,
        [Cuil] bigint NOT NULL,
        CONSTRAINT [PK_QuizCuilVinculado]
            PRIMARY KEY ([QuizSesionId], [Cuil]),
        CONSTRAINT [FK_QuizCuilVinculado_QuizSesion_QuizSesionId]
            FOREIGN KEY ([QuizSesionId])
            REFERENCES [dbo].[QuizSesion] ([Id])
            ON DELETE CASCADE
    );

    CREATE TABLE [dbo].[QuizDesafio] (
        [Id] bigint IDENTITY(1,1) NOT NULL,
        [QuizSesionId] bigint NOT NULL,
        [Numero] tinyint NOT NULL,
        [Escenario] tinyint NOT NULL,
        [EsActual] bit NOT NULL,
        [FechaCreacion] datetime NOT NULL,
        CONSTRAINT [PK_QuizDesafio]
            PRIMARY KEY ([Id]),
        CONSTRAINT [FK_QuizDesafio_QuizSesion_QuizSesionId]
            FOREIGN KEY ([QuizSesionId])
            REFERENCES [dbo].[QuizSesion] ([Id])
            ON DELETE CASCADE,
        CONSTRAINT [CK_QuizDesafio_Numero]
            CHECK ([Numero] > 0),
        CONSTRAINT [CK_QuizDesafio_Escenario]
            CHECK ([Escenario] BETWEEN 1 AND 4)
    );

    CREATE TABLE [dbo].[QuizOpcion] (
        [QuizDesafioId] bigint NOT NULL,
        [CodigoOpcion] tinyint NOT NULL,
        [Cuil] bigint NULL,
        [EsVinculado] bit NOT NULL,
        CONSTRAINT [PK_QuizOpcion]
            PRIMARY KEY ([QuizDesafioId], [CodigoOpcion]),
        CONSTRAINT [FK_QuizOpcion_QuizDesafio_QuizDesafioId]
            FOREIGN KEY ([QuizDesafioId])
            REFERENCES [dbo].[QuizDesafio] ([Id])
            ON DELETE CASCADE,
        CONSTRAINT [CK_QuizOpcion_Codigo]
            CHECK ([CodigoOpcion] BETWEEN 0 AND 5),
        CONSTRAINT [CK_QuizOpcion_Cuil]
            CHECK (
                (
                    [CodigoOpcion] BETWEEN 0 AND 3
                    AND [Cuil] IS NOT NULL
                )
                OR
                (
                    [CodigoOpcion] BETWEEN 4 AND 5
                    AND [Cuil] IS NULL
                )
            )
    );

    CREATE TABLE [dbo].[QuizRespuesta] (
        [Id] bigint IDENTITY(1,1) NOT NULL,
        [QuizDesafioId] bigint NOT NULL,
        [EsCorrecta] bit NOT NULL,
        [FechaRespuesta] datetime NOT NULL,
        CONSTRAINT [PK_QuizRespuesta]
            PRIMARY KEY ([Id]),
        CONSTRAINT [AK_QuizRespuesta_Id_QuizDesafioId]
            UNIQUE ([Id], [QuizDesafioId]),
        CONSTRAINT [FK_QuizRespuesta_QuizDesafio_QuizDesafioId]
            FOREIGN KEY ([QuizDesafioId])
            REFERENCES [dbo].[QuizDesafio] ([Id])
            ON DELETE CASCADE,
        CONSTRAINT [UX_QuizRespuesta_QuizDesafio]
            UNIQUE ([QuizDesafioId])
    );

    CREATE TABLE [dbo].[QuizRespuestaOpcion] (
        [QuizRespuestaId] bigint NOT NULL,
        [QuizDesafioId] bigint NOT NULL,
        [CodigoOpcion] tinyint NOT NULL,
        CONSTRAINT [PK_QuizRespuestaOpcion]
            PRIMARY KEY ([QuizRespuestaId], [CodigoOpcion]),
        CONSTRAINT [FK_QuizRespuestaOpcion_QuizRespuesta]
            FOREIGN KEY ([QuizRespuestaId], [QuizDesafioId])
            REFERENCES [dbo].[QuizRespuesta] ([Id], [QuizDesafioId])
            ON DELETE CASCADE,
        CONSTRAINT [FK_QuizRespuestaOpcion_QuizOpcion]
            FOREIGN KEY ([QuizDesafioId], [CodigoOpcion])
            REFERENCES [dbo].[QuizOpcion] (
                [QuizDesafioId],
                [CodigoOpcion]
            )
    );

    CREATE UNIQUE INDEX [UX_QuizDesafio_Sesion_Actual]
        ON [dbo].[QuizDesafio] ([QuizSesionId])
        WHERE [EsActual] = 1;

    CREATE UNIQUE INDEX [UX_QuizDesafio_Sesion_Numero]
        ON [dbo].[QuizDesafio] (
            [QuizSesionId],
            [Numero]
        );

    CREATE INDEX [IX_QuizSesion_Cuit_Bloqueo]
        ON [dbo].[QuizSesion] (
            [CuitEmpresa],
            [Estado],
            [BloqueadoHasta]
        );

    CREATE INDEX [IX_QuizSesion_Usuario_Fecha]
        ON [dbo].[QuizSesion] (
            [UsuarioId],
            [FechaCreacion]
        );

    INSERT INTO [dbo].[__EFMigrationsHistory] (
        [MigrationId],
        [ProductVersion]
    )
    VALUES (
        N'20260821120000_RebuildQuizPersistence',
        N'7.0.3'
    );
END;
GO

COMMIT;
GO
