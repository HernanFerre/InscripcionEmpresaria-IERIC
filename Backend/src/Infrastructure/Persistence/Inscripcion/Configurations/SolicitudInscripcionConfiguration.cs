using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IERIC.SumariosIERIC.Infrastructure.Persistence.Inscripcion.Configurations
{
    public class SolicitudInscripcionConfiguration
        : IEntityTypeConfiguration<SolicitudInscripcionEntity>
    {
        public void Configure(
            EntityTypeBuilder<SolicitudInscripcionEntity> builder
        )
        {
            builder.ToTable(
                "SolicitudInscripcionDigitalEmpresaria",
                "dbo"
            );

            builder.HasKey(x => x.Id)
                .HasName("PK_SolInscEmprDig")
                .IsClustered();

            builder.Property(x => x.Id)
                .HasColumnType("bigint")
                .UseIdentityColumn()
                .IsRequired();

            builder.Property(x => x.Idempresa)
                .HasColumnType("bigint")
                .IsRequired();

            builder.Property(x => x.UsuarioId)
                .HasColumnType("uniqueidentifier")
                .IsRequired();

            builder.Property(x => x.Comentarios)
                .HasColumnType("nvarchar(max)")
                .IsRequired(false);

            builder.Property(x => x.Ua)
                .HasColumnName("ua")
                .HasColumnType("varchar(23)")
                .HasMaxLength(23)
                .IsRequired(false);

            builder.Property(x => x.Um)
                .HasColumnName("um")
                .HasColumnType("varchar(23)")
                .HasMaxLength(23)
                .IsRequired(false);

            builder.Property(x => x.Fa)
                .HasColumnName("fa")
                .HasColumnType("datetime")
                .IsRequired(false);

            builder.Property(x => x.Fm)
                .HasColumnName("fm")
                .HasColumnType("datetime")
                .IsRequired(false);

            builder.HasOne<EmpresaEntity>()
                .WithMany()
                .HasForeignKey(x => x.Idempresa)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName(
                    "FK_SolInscEmprDig_Empresa"
                );
        }
    }
}