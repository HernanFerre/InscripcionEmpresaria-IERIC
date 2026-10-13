using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IERIC.SumariosIERIC.Infrastructure.Persistence.Inscripcion.Configurations
{
    public class RepresentanteConfiguration
        : IEntityTypeConfiguration<RepresentanteEntity>
    {
        public void Configure(
            EntityTypeBuilder<RepresentanteEntity> builder
        )
        {
            builder.ToTable(
                "Representante",
                "dbo"
            );

            builder.HasKey(x => x.Id)
                .HasName("PK_Representante")
                .IsClustered();

            builder.Property(x => x.Id)
                .HasColumnType("bigint")
                .UseIdentityColumn(1, 1)
                .IsRequired();

            builder.Property(x => x.IdEmpresa)
                .HasColumnType("bigint")
                .IsRequired();

            builder.Property(x => x.Apellido)
                .HasColumnType("varchar(100)")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Nombre)
                .HasColumnType("varchar(100)")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Cuil)
                .HasColumnType("bigint")
                .IsRequired();

            builder.Property(x => x.IdTipoRepresentante)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(x => x.Correo)
                .HasColumnType("varchar(254)")
                .HasMaxLength(254)
                .IsRequired();

            builder.Property(x => x.Telefono)
                .HasColumnType("varchar(30)")
                .HasMaxLength(30)
                .IsRequired();

            builder.HasIndex(x => new
            {
                x.IdEmpresa,
                x.Cuil
            })
                .IsUnique()
                .HasDatabaseName(
                    "UX_Representante_Empresa_Cuil"
                );

            builder.HasOne<EmpresaEntity>()
                .WithMany()
                .HasForeignKey(x => x.IdEmpresa)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName(
                    "FK_Representante_Empresa"
                );
        }
    }
}