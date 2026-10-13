using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IERIC.SumariosIERIC.Infrastructure.Persistence.Inscripcion.Configurations
{
    public class SolicitudRepresentanteConfiguration
        : IEntityTypeConfiguration<SolicitudRepresentanteEntity>
    {
        public void Configure(
            EntityTypeBuilder<SolicitudRepresentanteEntity> builder
        )
        {
            builder.ToTable(
                "SolicitudRepresentante",
                "dbo"
            );

            builder.HasKey(x => new
            {
                x.IdSolicitud,
                x.IdRepresentante
            })
                .HasName("PK_SolicitudRepresentante")
                .IsClustered();

            builder.Property(x => x.IdSolicitud)
                .HasColumnType("bigint")
                .ValueGeneratedNever()
                .IsRequired();

            builder.Property(x => x.IdRepresentante)
                .HasColumnType("bigint")
                .ValueGeneratedNever()
                .IsRequired();

            builder.HasOne<SolicitudInscripcionEntity>()
                .WithMany()
                .HasForeignKey(x => x.IdSolicitud)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName(
                    "FK_SolicitudRepresentante_Solicitud"
                );

            builder.HasOne<RepresentanteEntity>()
                .WithMany()
                .HasForeignKey(x => x.IdRepresentante)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName(
                    "FK_SolicitudRepresentante_Representante"
                );
        }
    }
}