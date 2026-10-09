using IERIC.SumariosIERIC.Infrastructure.Persistence.Inscripcion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IERIC.SumariosIERIC.Infrastructure.Persistence.Inscripcion.Configurations
{
    public class EmpresaConfiguration
        : IEntityTypeConfiguration<EmpresaEntity>
    {
        public void Configure(
            EntityTypeBuilder<EmpresaEntity> builder
        )
        {
            builder.ToTable(
                "Empresa",
                "dbo"
            );

            builder.HasKey(x => x.Id)
                .HasName("PK_Empresa")
                .IsClustered();

            builder.Property(x => x.Id)
                .HasColumnType("bigint")
                .HasColumnOrder(0)
                .UseIdentityColumn(1, 1)
                .IsRequired();

            builder.Property(x => x.RazonSocial)
                .HasColumnType("varchar(150)")
                .HasMaxLength(150)
                .HasColumnOrder(1)
                .IsRequired(false);

            builder.Property(x => x.Cuit)
                .HasColumnType("bigint")
                .HasColumnOrder(2)
                .ValueGeneratedNever()
                .IsRequired();

            builder.HasIndex(x => x.Cuit)
                .IsUnique()
                .HasDatabaseName("UX_Empresa_Cuit");

            builder.Property(x => x.EsCooperativa)
                .HasColumnType("bit")
                .HasColumnOrder(3)
                .IsRequired();

            builder.Property(x => x.EstadoActivo)
                .HasColumnType("bit")
                .HasColumnOrder(4)
                .IsRequired();

            builder.Property(x => x.LegacyId)
                .HasColumnType("int")
                .HasColumnOrder(5)
                .IsRequired();

            builder.Property(x => x.Activo)
                .HasColumnType("bit")
                .HasColumnOrder(6)
                .IsRequired();

            builder.Property(x => x.Fa)
                .HasColumnName("fa")
                .HasColumnType("datetime2(7)")
                .HasColumnOrder(7)
                .IsRequired();

            builder.Property(x => x.Ua)
                .HasColumnName("ua")
                .HasColumnType("varchar(23)")
                .HasMaxLength(23)
                .HasColumnOrder(8)
                .IsRequired(false);

            builder.Property(x => x.Fm)
                .HasColumnName("fm")
                .HasColumnType("datetime2(7)")
                .HasColumnOrder(9)
                .IsRequired();

            builder.Property(x => x.Um)
                .HasColumnName("um")
                .HasColumnType("varchar(23)")
                .HasMaxLength(23)
                .HasColumnOrder(10)
                .IsRequired(false);

            builder.Property(x => x.Calle)
                .HasColumnType("varchar(150)")
                .HasMaxLength(150)
                .HasColumnOrder(11)
                .HasDefaultValueSql("''")
                .IsRequired();

            builder.Property(x => x.Numero)
                .HasColumnType("varchar(20)")
                .HasMaxLength(20)
                .HasColumnOrder(12)
                .HasDefaultValueSql("''")
                .IsRequired();

            builder.Property(x => x.Piso)
                .HasColumnType("tinyint")
                .HasColumnOrder(13)
                .IsRequired(false);

            builder.Property(x => x.DeptoOficina)
                .HasColumnType("varchar(20)")
                .HasMaxLength(20)
                .HasColumnOrder(14)
                .IsRequired(false);

            builder.Property(x => x.CodigoPostal)
                .HasColumnType("varchar(10)")
                .HasMaxLength(10)
                .HasColumnOrder(15)
                .HasDefaultValueSql("''")
                .IsRequired();

            builder.Property(x => x.IdProvincia)
                .HasColumnType("int")
                .HasColumnOrder(16)
                .IsRequired(false);

            builder.Property(x => x.IdLocalidad)
                .HasColumnType("int")
                .HasColumnOrder(17)
                .IsRequired(false);

            builder.Property(x => x.Correo)
                .HasColumnType("varchar(254)")
                .HasMaxLength(254)
                .HasColumnOrder(18)
                .HasDefaultValueSql("''")
                .IsRequired();

            builder.Property(x => x.Telefono)
                .HasColumnType("varchar(30)")
                .HasMaxLength(30)
                .HasColumnOrder(19)
                .IsRequired(false);

            builder.Property(x => x.IdActividadsolicitud)
                .HasColumnType("int")
                .HasColumnOrder(20)
                .HasDefaultValueSql("''")
                .IsRequired();

            builder.Property(x => x.IdCaracter)
                .HasColumnType("int")
                .HasColumnOrder(21)
                .HasDefaultValueSql("''")
                .IsRequired();

            builder.Property(x => x.IdTipoSoc)
                .HasColumnType("int")
                .HasColumnOrder(22)
                .HasDefaultValueSql("''")
                .IsRequired();

            builder.Property(x => x.DDJJ)
                .HasColumnType("bit")
                .HasColumnOrder(23)
                .IsRequired();
        }
    }
}