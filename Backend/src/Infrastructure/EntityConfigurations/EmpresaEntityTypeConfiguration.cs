using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using IERIC.SumariosIERIC.Domain.Entities;
using IERIC.SumariosIERIC.Domain.ValueObjects.Network;

namespace IERIC.SumariosIERIC.Infrastructure.EntityConfigurations
{
    class EmpresaEntityTypeConfiguration : IEntityTypeConfiguration<Empresa>
    {
        public void Configure(EntityTypeBuilder<Empresa> EmpresaConfiguration)
        {
            EmpresaConfiguration.ToTable("Empresa", SumariosContext.DEFAULT_SCHEMA);

            EmpresaConfiguration.HasKey(o => o.Id);

            EmpresaConfiguration.Ignore(b => b.DomainEvents);

            var empresaConverter = new ValueConverter<Cuit, Int64>(from => from, to => (Cuit)to);
            EmpresaConfiguration.Property(p => p.Cuit).HasConversion(empresaConverter).HasColumnType("bigint");

        }
    }
}