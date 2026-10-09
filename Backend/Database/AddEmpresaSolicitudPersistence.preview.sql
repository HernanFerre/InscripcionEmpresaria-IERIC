BEGIN TRANSACTION;
GO

DROP INDEX [IX_QuizRespuestaOpcion_QuizDesafioId_CodigoOpcion] ON [dbo].[QuizRespuestaOpcion];
GO

DROP INDEX [IX_QuizRespuestaOpcion_QuizRespuestaId_QuizDesafioId] ON [dbo].[QuizRespuestaOpcion];
GO

CREATE TABLE [dbo].[Empresa] (
    [Id] bigint NOT NULL IDENTITY,
    [RazonSocial] varchar(150) NULL,
    [Cuit] bigint NOT NULL,
    [EsCooperativa] bit NOT NULL,
    [EstadoActivo] bit NOT NULL,
    [LegacyId] int NOT NULL,
    [Activo] bit NOT NULL,
    [fa] datetime2(7) NOT NULL,
    [ua] varchar(23) NULL,
    [fm] datetime2(7) NOT NULL,
    [um] varchar(23) NULL,
    [Calle] varchar(150) NOT NULL DEFAULT (''),
    [Numero] varchar(20) NOT NULL DEFAULT (''),
    [Piso] tinyint NULL,
    [DeptoOficina] varchar(20) NULL,
    [CodigoPostal] varchar(10) NOT NULL DEFAULT (''),
    [Provincia] varchar(100) NOT NULL DEFAULT (''),
    [Localidad] varchar(150) NOT NULL DEFAULT (''),
    [Correo] varchar(254) NOT NULL DEFAULT (''),
    [Telefono] varchar(30) NULL,
    [IdActividadsolicitud] int NOT NULL DEFAULT (''),
    [IdCaracter] int NOT NULL DEFAULT (''),
    [IdTipoSoc] int NOT NULL DEFAULT (''),
    [DDJJ] bit NOT NULL,
    CONSTRAINT [PK_Empresa] PRIMARY KEY CLUSTERED ([Id])
);
GO

CREATE TABLE [dbo].[EmpresasCuit] (
    [IdEmpresa] bigint NOT NULL,
    [IdEmpresaIntegrante] bigint NOT NULL,
    CONSTRAINT [PK_EmpresasCuit] PRIMARY KEY CLUSTERED ([IdEmpresa], [IdEmpresaIntegrante]),
    CONSTRAINT [FK_EmpresasCuit_EmpresaIntegrante] FOREIGN KEY ([IdEmpresaIntegrante]) REFERENCES [dbo].[Empresa] ([Id]),
    CONSTRAINT [FK_EmpresasCuit_EmpresaPrincipal] FOREIGN KEY ([IdEmpresa]) REFERENCES [dbo].[Empresa] ([Id])
);
GO

CREATE TABLE [dbo].[SolicitudInscripcionDigitalEmpresaria] (
    [Id] bigint NOT NULL IDENTITY,
    [Idempresa] bigint NOT NULL,
    [UsuarioId] uniqueidentifier NOT NULL,
    [Comentarios] nvarchar(max) NULL,
    [ua] varchar(23) NULL,
    [um] varchar(23) NULL,
    [fa] datetime NULL,
    [fm] datetime NULL,
    CONSTRAINT [PK_SolInscEmprDig] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_SolInscEmprDig_Empresa] FOREIGN KEY ([Idempresa]) REFERENCES [dbo].[Empresa] ([Id])
);
GO

CREATE UNIQUE INDEX [UX_Empresa_Cuit] ON [dbo].[Empresa] ([Cuit]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261009150438_AddEmpresaSolicitudPersistence', N'7.0.3');
GO

COMMIT;
GO

