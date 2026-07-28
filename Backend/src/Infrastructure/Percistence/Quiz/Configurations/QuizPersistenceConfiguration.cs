using IERIC.SumariosIERIC.Infrastructure.Persistence.Quiz;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IERIC.SumariosIERIC.Infrastructure.Persistence.Quiz.Configurations
{
    public class QuizSesionConfiguration : IEntityTypeConfiguration<QuizSesionEntity>
    {
        public void Configure(EntityTypeBuilder<QuizSesionEntity> builder)
        {
            builder.ToTable("QuizSesion");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.CuitEmpresa)
                .HasColumnType("char(11)")
                .HasMaxLength(11)
                .IsFixedLength()
                .IsRequired();

            builder.Property(x => x.Estado)
                .IsRequired();

            builder.Property(x => x.IntentosTotales)
                .IsRequired();

            builder.Property(x => x.IntentosRestantes)
                .IsRequired();

            builder.Property(x => x.FechaCreacionUtc)
                .HasColumnType("datetime2")
                .IsRequired();

            builder.Property(x => x.FechaExpiracionUtc)
                .HasColumnType("datetime2")
                .IsRequired();

            builder.Property(x => x.FechaFinalizacionUtc)
                .HasColumnType("datetime2");

            builder.HasIndex(x => x.CuitEmpresa)
                .HasDatabaseName("IX_QuizSesion_CuitEmpresa");

            builder.HasIndex(x => x.Estado)
                .HasDatabaseName("IX_QuizSesion_Estado");
        }
    }

    public class QuizCuilVinculadoConfiguration
        : IEntityTypeConfiguration<QuizCuilVinculadoEntity>
    {
        public void Configure(EntityTypeBuilder<QuizCuilVinculadoEntity> builder)
        {
            builder.ToTable("QuizCuilVinculado");

            builder.HasKey(x => new
            {
                x.QuizId,
                x.Cuil
            });

            builder.Property(x => x.Cuil)
                .HasColumnType("char(11)")
                .HasMaxLength(11)
                .IsFixedLength()
                .IsRequired();

            builder.HasOne(x => x.Quiz)
                .WithMany(x => x.CuilesVinculados)
                .HasForeignKey(x => x.QuizId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class QuizDesafioConfiguration
        : IEntityTypeConfiguration<QuizDesafioEntity>
    {
        public void Configure(EntityTypeBuilder<QuizDesafioEntity> builder)
        {
            builder.ToTable("QuizDesafio");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.Numero)
                .IsRequired();

            builder.Property(x => x.Escenario)
                .IsRequired();

            builder.Property(x => x.EsActual)
                .IsRequired();

            builder.Property(x => x.FechaCreacionUtc)
                .HasColumnType("datetime2")
                .IsRequired();

            builder.HasOne(x => x.Quiz)
                .WithMany(x => x.Desafios)
                .HasForeignKey(x => x.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new
            {
                x.QuizId,
                x.Numero
            })
                .IsUnique()
                .HasDatabaseName("UX_QuizDesafio_Quiz_Numero");

            builder.HasIndex(x => x.QuizId)
                .IsUnique()
                .HasFilter("[EsActual] = 1")
                .HasDatabaseName("UX_QuizDesafio_Quiz_Actual");
        }
    }

    public class QuizOpcionConfiguration
        : IEntityTypeConfiguration<QuizOpcionEntity>
    {
        public void Configure(EntityTypeBuilder<QuizOpcionEntity> builder)
        {
            builder.ToTable("QuizOpcion");

            builder.HasKey(x => new
            {
                x.QuizDesafioId,
                x.Id
            });

            builder.Property(x => x.Id)
                .HasColumnType("varchar(10)")
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(x => x.Cuil)
                .HasColumnType("char(11)")
                .HasMaxLength(11)
                .IsFixedLength()
                .IsRequired();

            builder.Property(x => x.EsVinculado)
                .IsRequired();

            builder.Property(x => x.Orden)
                .IsRequired();

            builder.HasOne(x => x.Desafio)
                .WithMany(x => x.Opciones)
                .HasForeignKey(x => x.QuizDesafioId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new
            {
                x.QuizDesafioId,
                x.Orden
            })
                .IsUnique()
                .HasDatabaseName("UX_QuizOpcion_Desafio_Orden");
        }
    }

    public class QuizRespuestaConfiguration
        : IEntityTypeConfiguration<QuizRespuestaEntity>
    {
        public void Configure(EntityTypeBuilder<QuizRespuestaEntity> builder)
        {
            builder.ToTable("QuizRespuesta");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.OpcionesSeleccionadas)
                .HasColumnType("nvarchar(200)")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.EsCorrecta)
                .IsRequired();

            builder.Property(x => x.FechaRespuestaUtc)
                .HasColumnType("datetime2")
                .IsRequired();

            builder.HasOne(x => x.Desafio)
                .WithMany(x => x.Respuestas)
                .HasForeignKey(x => x.QuizDesafioId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new
            {
                x.QuizDesafioId,
                x.FechaRespuestaUtc
            })
                .HasDatabaseName("IX_QuizRespuesta_Desafio_Fecha");
        }
    }
}