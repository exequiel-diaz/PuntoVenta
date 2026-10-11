
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Infrastructure.Persistence.Configurations
{
    public class ClienteEmpresaConfiguration
        : IEntityTypeConfiguration<ClienteEmpresa>
    {
        public void Configure(EntityTypeBuilder<ClienteEmpresa> builder)
        {
            builder.Property(c => c.RazonSocial)
                .HasMaxLength(150);

            builder.Property(c => c.Cuit)
                .HasColumnName("Cuit")
                .HasMaxLength(20);

            builder.Property(c => c.Contacto)
                .HasMaxLength(150);
        }
    }
}
