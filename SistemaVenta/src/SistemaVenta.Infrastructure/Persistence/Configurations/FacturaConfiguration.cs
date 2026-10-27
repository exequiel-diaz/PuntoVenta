
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Infrastructure.Persistence.Configurations
{
    public class FacturaConfiguration
        : IEntityTypeConfiguration<Factura>
    {
        public void Configure(EntityTypeBuilder<Factura> builder)
        {

            builder.Property(f => f.ImporteTotal)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(f => f.Tipo)
                .HasConversion<string>()
                .HasMaxLength(10)
                .IsRequired();

            builder.HasOne<Venta>()
                .WithMany()
                .HasForeignKey(f => f.VentaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
