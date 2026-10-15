using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IERIC.SumariosIERIC.Infrastructure.Persistence.Inscripcion.Configurations
{
    public class TrabajadorNominaConfiguration
        : IEntityTypeConfiguration<TrabajadorNominaEntity>
    {
        public void Configure(
            EntityTypeBuilder<TrabajadorNominaEntity> builder
        )
        {
            builder.ToTable(
                "TrabajadorNomina",
                "dbo"
            );

            builder.HasKey(x => x.Id)
                .HasName("PK_TrabajadorNomina")
                .IsClustered();

            builder.Property(x => x.Id)
                .HasColumnType("bigint")
                .UseIdentityColumn(1, 1)
                .IsRequired();

            builder.Property(x => x.IdSolicitud)
                .HasColumnType("bigint")
                .IsRequired();

            builder.Property(x => x.ApellidoYNombre)
                .HasColumnType("varchar(200)")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Cuil)
                .HasColumnType("varchar(11)")
                .HasMaxLength(11)
                .IsRequired();

            builder.Property(x => x.Ingreso)
                .HasColumnType("date")
                .IsRequired();

            builder.Property(x => x.Egreso)
                .HasColumnType("date")
                .IsRequired(false);

            builder.Property(x => x.PerteneceAConstr)
                .HasColumnType("bit")
                .IsRequired();

            builder.Property(x => x.EsManual)
                .HasColumnType("bit")
                .HasDefaultValue(false)
                .IsRequired();

            builder.HasIndex(x => x.IdSolicitud)
                .HasDatabaseName(
                    "IX_TrabajadorNomina_IdSolicitud"
                );

            builder.HasOne<SolicitudInscripcionEntity>()
                .WithMany()
                .HasForeignKey(x => x.IdSolicitud)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName(
                    "FK_TrabajadorNomina_Solicitud"
                );
        }
    }
}