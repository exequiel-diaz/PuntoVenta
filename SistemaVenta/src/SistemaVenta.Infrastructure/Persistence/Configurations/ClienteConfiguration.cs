
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Infrastructure.Persistence.Configurations
{
    public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
    {
        public void Configure(EntityTypeBuilder<Cliente> builder)
        {
            

            builder.ToTable("Clientes");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Id)
                .ValueGeneratedOnAdd();

            builder.Property(c => c.Telefono)
                .IsRequired()
                .HasMaxLength(30);

            builder.Property(c => c.Email)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(c => c.Direccion)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(c => c.Observaciones)
                .HasMaxLength(500);

            builder.Property(c => c.Estado)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.HasDiscriminator<string>("TipoCliente")
                .HasValue<ClienteFisico>("Fisico")
                .HasValue<ClienteEmpresa>("Empresa");

            
        }
    }
}
