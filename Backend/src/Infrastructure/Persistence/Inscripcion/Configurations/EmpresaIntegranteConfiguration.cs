using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IERIC.SumariosIERIC.Infrastructure.Persistence.Inscripcion.Configurations
{
    public class EmpresaIntegranteConfiguration
        : IEntityTypeConfiguration<EmpresaIntegranteEntity>
    {
        public void Configure(
            EntityTypeBuilder<EmpresaIntegranteEntity> builder
        )
        {
            builder.ToTable(
                "EmpresasCuit",
                "dbo"
            );

            builder.HasKey(x => new
            {
                x.IdEmpresa,
                x.IdEmpresaIntegrante
            })
                .HasName("PK_EmpresasCuit")
                .IsClustered();

            builder.Property(x => x.IdEmpresa)
                .HasColumnType("bigint")
                .ValueGeneratedNever()
                .IsRequired();

            builder.Property(x => x.IdEmpresaIntegrante)
                .HasColumnType("bigint")
                .ValueGeneratedNever()
                .IsRequired();

            builder.HasOne<EmpresaEntity>()
                .WithMany()
                .HasForeignKey(x => x.IdEmpresa)
                .HasPrincipalKey(x => x.Cuit)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName(
                    "FK_EmpresasCuit_Empresa"
                );
        }
    }
}