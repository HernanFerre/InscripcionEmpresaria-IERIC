/*
    ============================================================================
    BASE DE DATOS: SolicitudInscripcionEmpresariaDigital
    OBJETIVO:      Crear la base y la estructura normalizada de persistencia.
    MOTOR:         Microsoft SQL Server
    ============================================================================

    Este script:
      - Crea la base de datos si todavía no existe.
      - No elimina ni reemplaza una base existente.
      - Crea la tabla Empresa.
      - Crea las seis tablas que componen la persistencia del quiz.
      - Crea claves primarias, claves foráneas, restricciones e índices.
      - Registra la línea base en __EFMigrationsHistory.

    Convenciones funcionales:

      QuizSesion.Estado
        1 = Activo
        2 = Validado
        3 = Bloqueado
        4 = Expirado

      QuizDesafio.Escenario
        1 = Una opción correcta
        2 = Dos opciones correctas
        3 = Todas las opciones correctas
        4 = Ninguna opción correcta

      QuizOpcion.CodigoOpcion
        0 = A
        1 = B
        2 = C
        3 = D
        4 = Ninguna
        5 = Todas

    Consideraciones:
      - Las fechas del módulo de quiz se almacenan como datetime, por decisión
        de diseño del proyecto.
      - IntentosRestantes no se persiste: se obtiene a partir de los intentos
        totales y las respuestas registradas.
      - QuizRespuestaOpcion almacena una fila atómica por cada opción elegida.
      - El DBA puede adaptar opciones físicas de la base (archivos, crecimiento,
        recovery model, collation, etc.) al ambiente de destino.
*/

USE [master];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF DB_ID(N'SolicitudInscripcionEmpresariaDigital') IS NULL
BEGIN
    EXEC(N'CREATE DATABASE [SolicitudInscripcionEmpresariaDigital]');
END;
GO

USE [SolicitudInscripcionEmpresariaDigital];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    /* ========================================================================
       HISTORIAL DE ENTITY FRAMEWORK
       ======================================================================== */

    IF OBJECT_ID(N'[dbo].[__EFMigrationsHistory]', N'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[__EFMigrationsHistory] (
            [MigrationId] nvarchar(150) NOT NULL,
            [ProductVersion] nvarchar(32) NOT NULL,

            CONSTRAINT [PK___EFMigrationsHistory]
                PRIMARY KEY ([MigrationId])
        );
    END;

    /* ========================================================================
       EMPRESA
       Entidad existente del proyecto, conservada sin cambios funcionales.
       ======================================================================== */

    IF OBJECT_ID(N'[dbo].[Empresa]', N'U') IS NULL
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

    /* ========================================================================
       QUIZSESION
       Raíz de cada proceso de validación de identidad de una empresa.
       ======================================================================== */

    IF OBJECT_ID(N'[dbo].[QuizSesion]', N'U') IS NULL
    BEGIN
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
    END;

    /* ========================================================================
       QUIZCUILVINCULADO
       CUILes reales obtenidos para la empresa al crear la sesión.
       ======================================================================== */

    IF OBJECT_ID(N'[dbo].[QuizCuilVinculado]', N'U') IS NULL
    BEGIN
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
    END;

    /* ========================================================================
       QUIZDESAFIO
       Un desafío por cada intento presentado dentro de la sesión.
       ======================================================================== */

    IF OBJECT_ID(N'[dbo].[QuizDesafio]', N'U') IS NULL
    BEGIN
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
    END;

    /* ========================================================================
       QUIZOPCION
       Opciones A-D y opciones especiales Ninguna/Todas de cada desafío.
       ======================================================================== */

    IF OBJECT_ID(N'[dbo].[QuizOpcion]', N'U') IS NULL
    BEGIN
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
    END;

    /* ========================================================================
       QUIZRESPUESTA
       Cabecera de la respuesta emitida para un desafío.
       La restricción UNIQUE garantiza una única respuesta por desafío.
       ======================================================================== */

    IF OBJECT_ID(N'[dbo].[QuizRespuesta]', N'U') IS NULL
    BEGIN
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
    END;

    /* ========================================================================
       QUIZRESPUESTAOPCION
       Detalle normalizado: una fila por cada opción elegida en la respuesta.
       Las claves foráneas garantizan que la opción pertenece al desafío.
       ======================================================================== */

    IF OBJECT_ID(N'[dbo].[QuizRespuestaOpcion]', N'U') IS NULL
    BEGIN
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
                REFERENCES [dbo].[QuizOpcion] ([QuizDesafioId], [CodigoOpcion])
                ON DELETE NO ACTION
        );
    END;

    /* ========================================================================
       ÍNDICES DEL MÓDULO DE QUIZ
       ======================================================================== */

    IF NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE [object_id] = OBJECT_ID(N'[dbo].[QuizDesafio]')
          AND [name] = N'UX_QuizDesafio_Sesion_Actual'
    )
    BEGIN
        CREATE UNIQUE INDEX [UX_QuizDesafio_Sesion_Actual]
            ON [dbo].[QuizDesafio] ([QuizSesionId])
            WHERE [EsActual] = 1;
    END;

    IF NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE [object_id] = OBJECT_ID(N'[dbo].[QuizDesafio]')
          AND [name] = N'UX_QuizDesafio_Sesion_Numero'
    )
    BEGIN
        CREATE UNIQUE INDEX [UX_QuizDesafio_Sesion_Numero]
            ON [dbo].[QuizDesafio] ([QuizSesionId], [Numero]);
    END;

    IF NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE [object_id] = OBJECT_ID(N'[dbo].[QuizSesion]')
          AND [name] = N'IX_QuizSesion_Cuit_Bloqueo'
    )
    BEGIN
        CREATE INDEX [IX_QuizSesion_Cuit_Bloqueo]
            ON [dbo].[QuizSesion] (
                [CuitEmpresa],
                [Estado],
                [BloqueadoHasta]
            );
    END;

    IF NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE [object_id] = OBJECT_ID(N'[dbo].[QuizSesion]')
          AND [name] = N'IX_QuizSesion_Usuario_Fecha'
    )
    BEGIN
        CREATE INDEX [IX_QuizSesion_Usuario_Fecha]
            ON [dbo].[QuizSesion] ([UsuarioId], [FechaCreacion]);
    END;

    /* ========================================================================
       LÍNEA BASE DE ENTITY FRAMEWORK
       ======================================================================== */

    IF NOT EXISTS (
        SELECT 1
        FROM [dbo].[__EFMigrationsHistory]
        WHERE [MigrationId] = N'20260821120000_RebuildQuizPersistence'
    )
    BEGIN
        INSERT INTO [dbo].[__EFMigrationsHistory] (
            [MigrationId],
            [ProductVersion]
        )
        VALUES (
            N'20260821120000_RebuildQuizPersistence',
            N'7.0.3'
        );
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
    BEGIN
        ROLLBACK TRANSACTION;
    END;

    THROW;
END CATCH;
GO

/* Verificación resumida de los objetos creados. */
SELECT
    [schema] = SCHEMA_NAME([schema_id]),
    [tabla] = [name]
FROM sys.tables
WHERE [name] IN (
    N'__EFMigrationsHistory',
    N'Empresa',
    N'QuizSesion',
    N'QuizCuilVinculado',
    N'QuizDesafio',
    N'QuizOpcion',
    N'QuizRespuesta',
    N'QuizRespuestaOpcion'
)
ORDER BY [name];
GO
