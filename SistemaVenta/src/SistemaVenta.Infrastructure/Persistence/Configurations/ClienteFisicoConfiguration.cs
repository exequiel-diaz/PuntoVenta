
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Infrastructure.Persistence.Configurations
{
    public class ClienteFisicoConfiguration
        : IEntityTypeConfiguration<ClienteFisico>
    {
        public void Configure(EntityTypeBuilder<ClienteFisico> builder)
        {
            builder.Property(c => c.Nombre)
                .HasMaxLength(100);

            builder.Property(c => c.Apellido)
                .HasMaxLength(100);

            builder.Property(c => c.Dni)
                .HasMaxLength(20);

            builder.Property(c => c.Cuit)
                .HasColumnName("Cuit")
                .HasMaxLength(20);

            builder.Property(c => c.FechaNacimiento)
                .HasColumnType("date");

            builder.HasIndex(c => c.Dni);
        }
    }
}
