using IERIC.SumariosIERIC.Infrastructure.Persistence.Quiz;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IERIC.SumariosIERIC.Infrastructure.Persistence.Quiz.Configurations
{
    public class QuizSesionConfiguration
        : IEntityTypeConfiguration<QuizSesionEntity>
    {
        public void Configure(
            EntityTypeBuilder<QuizSesionEntity> builder
        )
        {
            builder.ToTable(
                "QuizSesion",
                "dbo",
                table =>
                {
                    table.HasCheckConstraint(
                        "CK_QuizSesion_Estado",
                        "[Estado] BETWEEN 1 AND 4"
                    );

                    table.HasCheckConstraint(
                        "CK_QuizSesion_IntentosTotales",
                        "[IntentosTotales] > 0"
                    );
                }
            );

            builder.HasKey(x => x.Id)
                .HasName("PK_QuizSesion");

            builder.Property(x => x.Id)
                .HasColumnType("bigint")
                .UseIdentityColumn();

            builder.Property(x => x.CuitEmpresa)
                .HasColumnType("bigint")
                .IsRequired();

            builder.Property(x => x.UsuarioId)
                .HasColumnType("nvarchar(200)")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Estado)
                .HasColumnType("tinyint")
                .IsRequired();

            builder.Property(x => x.IntentosTotales)
                .HasColumnType("tinyint")
                .IsRequired();

            builder.Property(x => x.FechaCreacion)
                .HasColumnType("datetime")
                .IsRequired();

            builder.Property(x => x.FechaExpiracion)
                .HasColumnType("datetime")
                .IsRequired();

            builder.Property(x => x.FechaFinalizacion)
                .HasColumnType("datetime");

            builder.Property(x => x.BloqueadoHasta)
                .HasColumnType("datetime");

            builder.HasIndex(x => new
            {
                x.CuitEmpresa,
                x.Estado,
                x.BloqueadoHasta
            })
                .HasDatabaseName(
                    "IX_QuizSesion_Cuit_Bloqueo"
                );

            builder.HasIndex(x => new
            {
                x.UsuarioId,
                x.FechaCreacion
            })
                .HasDatabaseName(
                    "IX_QuizSesion_Usuario_Fecha"
                );
        }
    }

    public class QuizCuilVinculadoConfiguration
        : IEntityTypeConfiguration<QuizCuilVinculadoEntity>
    {
        public void Configure(
            EntityTypeBuilder<QuizCuilVinculadoEntity> builder
        )
        {
            builder.ToTable(
                "QuizCuilVinculado",
                "dbo"
            );

            builder.HasKey(x => new
            {
                x.QuizSesionId,
                x.Cuil
            })
                .HasName("PK_QuizCuilVinculado");

            builder.Property(x => x.QuizSesionId)
                .HasColumnType("bigint")
                .IsRequired();

            builder.Property(x => x.Cuil)
                .HasColumnType("bigint")
                .IsRequired();

            builder.HasOne(x => x.QuizSesion)
                .WithMany(x => x.CuilesVinculados)
                .HasForeignKey(x => x.QuizSesionId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName(
                    "FK_QuizCuilVinculado_QuizSesion_QuizSesionId"
                );
        }
    }

    public class QuizDesafioConfiguration
        : IEntityTypeConfiguration<QuizDesafioEntity>
    {
        public void Configure(
            EntityTypeBuilder<QuizDesafioEntity> builder
        )
        {
            builder.ToTable(
                "QuizDesafio",
                "dbo",
                table =>
                {
                    table.HasCheckConstraint(
                        "CK_QuizDesafio_Numero",
                        "[Numero] > 0"
                    );

                    table.HasCheckConstraint(
                        "CK_QuizDesafio_Escenario",
                        "[Escenario] BETWEEN 1 AND 4"
                    );
                }
            );

            builder.HasKey(x => x.Id)
                .HasName("PK_QuizDesafio");

            builder.Property(x => x.Id)
                .HasColumnType("bigint")
                .UseIdentityColumn();

            builder.Property(x => x.QuizSesionId)
                .HasColumnType("bigint")
                .IsRequired();

            builder.Property(x => x.Numero)
                .HasColumnType("tinyint")
                .IsRequired();

            builder.Property(x => x.Escenario)
                .HasColumnType("tinyint")
                .IsRequired();

            builder.Property(x => x.EsActual)
                .HasColumnType("bit")
                .IsRequired();

            builder.Property(x => x.FechaCreacion)
                .HasColumnType("datetime")
                .IsRequired();

            builder.HasOne(x => x.QuizSesion)
                .WithMany(x => x.Desafios)
                .HasForeignKey(x => x.QuizSesionId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName(
                    "FK_QuizDesafio_QuizSesion_QuizSesionId"
                );

            builder.HasIndex(x => new
            {
                x.QuizSesionId,
                x.Numero
            })
                .IsUnique()
                .HasDatabaseName(
                    "UX_QuizDesafio_Sesion_Numero"
                );

            builder.HasIndex(x => x.QuizSesionId)
                .IsUnique()
                .HasFilter("[EsActual] = 1")
                .HasDatabaseName(
                    "UX_QuizDesafio_Sesion_Actual"
                );
        }
    }

    public class QuizOpcionConfiguration
        : IEntityTypeConfiguration<QuizOpcionEntity>
    {
        public void Configure(
            EntityTypeBuilder<QuizOpcionEntity> builder
        )
        {
            builder.ToTable(
                "QuizOpcion",
                "dbo",
                table =>
                {
                    table.HasCheckConstraint(
                        "CK_QuizOpcion_Codigo",
                        "[CodigoOpcion] BETWEEN 0 AND 5"
                    );

                    table.HasCheckConstraint(
                        "CK_QuizOpcion_Cuil",
                        @"(
                            (
                                [CodigoOpcion] BETWEEN 0 AND 3
                                AND [Cuil] IS NOT NULL
                            )
                            OR
                            (
                                [CodigoOpcion] BETWEEN 4 AND 5
                                AND [Cuil] IS NULL
                            )
                        )"
                    );
                }
            );

            builder.HasKey(x => new
            {
                x.QuizDesafioId,
                x.CodigoOpcion
            })
                .HasName("PK_QuizOpcion");

            builder.Property(x => x.QuizDesafioId)
                .HasColumnType("bigint")
                .IsRequired();

            builder.Property(x => x.CodigoOpcion)
                .HasColumnType("tinyint")
                .IsRequired();

            builder.Property(x => x.Cuil)
                .HasColumnType("bigint");

            builder.Property(x => x.EsVinculado)
                .HasColumnType("bit")
                .IsRequired();

            builder.HasOne(x => x.Desafio)
                .WithMany(x => x.Opciones)
                .HasForeignKey(x => x.QuizDesafioId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName(
                    "FK_QuizOpcion_QuizDesafio_QuizDesafioId"
                );
        }
    }

    public class QuizRespuestaConfiguration
        : IEntityTypeConfiguration<QuizRespuestaEntity>
    {
        public void Configure(
            EntityTypeBuilder<QuizRespuestaEntity> builder
        )
        {
            builder.ToTable(
                "QuizRespuesta",
                "dbo"
            );

            builder.HasKey(x => x.Id)
                .HasName("PK_QuizRespuesta");

            builder.HasAlternateKey(x => new
            {
                x.Id,
                x.QuizDesafioId
            })
                .HasName(
                    "AK_QuizRespuesta_Id_QuizDesafioId"
                );

            builder.HasAlternateKey(x => x.QuizDesafioId)
                .HasName(
                    "UX_QuizRespuesta_QuizDesafio"
                );

            builder.Property(x => x.Id)
                .HasColumnType("bigint")
                .UseIdentityColumn();

            builder.Property(x => x.QuizDesafioId)
                .HasColumnType("bigint")
                .IsRequired();

            builder.Property(x => x.EsCorrecta)
                .HasColumnType("bit")
                .IsRequired();

            builder.Property(x => x.FechaRespuesta)
                .HasColumnType("datetime")
                .IsRequired();

            builder.HasOne(x => x.Desafio)
                .WithOne(x => x.Respuesta)
                .HasForeignKey<QuizRespuestaEntity>(
                    x => x.QuizDesafioId
                )
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName(
                    "FK_QuizRespuesta_QuizDesafio_QuizDesafioId"
                );
        }
    }

    public class QuizRespuestaOpcionConfiguration
        : IEntityTypeConfiguration<QuizRespuestaOpcionEntity>
    {
        public void Configure(
            EntityTypeBuilder<QuizRespuestaOpcionEntity> builder
        )
        {
            builder.ToTable(
                "QuizRespuestaOpcion",
                "dbo"
            );

            builder.HasKey(x => new
            {
                x.QuizRespuestaId,
                x.CodigoOpcion
            })
                .HasName("PK_QuizRespuestaOpcion");

            builder.Property(x => x.QuizRespuestaId)
                .HasColumnType("bigint")
                .IsRequired();

            builder.Property(x => x.QuizDesafioId)
                .HasColumnType("bigint")
                .IsRequired();

            builder.Property(x => x.CodigoOpcion)
                .HasColumnType("tinyint")
                .IsRequired();

            builder.HasOne(x => x.Respuesta)
                .WithMany(x => x.OpcionesSeleccionadas)
                .HasForeignKey(x => new
                {
                    x.QuizRespuestaId,
                    x.QuizDesafioId
                })
                .HasPrincipalKey(x => new
                {
                    x.Id,
                    x.QuizDesafioId
                })
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName(
                    "FK_QuizRespuestaOpcion_QuizRespuesta"
                );

            builder.HasOne(x => x.Opcion)
                .WithMany(x => x.RespuestasSeleccionadas)
                .HasForeignKey(x => new
                {
                    x.QuizDesafioId,
                    x.CodigoOpcion
                })
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName(
                    "FK_QuizRespuestaOpcion_QuizOpcion"
                );
        }
    }
}