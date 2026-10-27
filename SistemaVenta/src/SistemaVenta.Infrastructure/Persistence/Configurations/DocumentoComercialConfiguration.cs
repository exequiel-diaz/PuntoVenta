
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Infrastructure.Persistence.Configurations
{
    public class DocumentoComercialConfiguration
        : IEntityTypeConfiguration<DocumentoComercial>
    {
        public void Configure(
            EntityTypeBuilder<DocumentoComercial> builder)
        {
            builder.ToTable("DocumentosComerciales");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.Numero)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(d => d.FechaEmision)
                .IsRequired();

            builder.Property(d => d.Estado)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(d => d.Observaciones)
                .HasMaxLength(500);

            builder.HasDiscriminator<string>("TipoDocumento")
                .HasValue<Factura>("Factura")
                .HasValue<Ticket>("Ticket")
                .HasValue<NotaCredito>("NotaCredito")
                .HasValue<NotaDebito>("NotaDebito")
                .HasValue<Recibo>("Recibo");

            builder.HasIndex(d => d.Numero);
        }
    }
}
