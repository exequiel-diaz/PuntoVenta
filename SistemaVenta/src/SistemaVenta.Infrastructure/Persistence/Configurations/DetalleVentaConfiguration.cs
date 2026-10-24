
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Infrastructure.Persistence.Configurations
{
    public class DetalleVentaConfiguration
        : IEntityTypeConfiguration<DetalleVenta>
    {
        public void Configure(EntityTypeBuilder<DetalleVenta> builder)
        {
            builder.ToTable("DetallesVenta");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.VentaId)
                .IsRequired();

            builder.Property(d => d.ProductoId)
                .IsRequired();

            builder.Property(d => d.Cantidad)
                .HasPrecision(18, 3)
                .IsRequired();

            builder.Property(d => d.PrecioUnitario)
                .HasPrecision(18, 2)
                .IsRequired();

            // El subtotal se calcula automáticamente.
            builder.Ignore(d => d.Subtotal);

            builder.HasOne(d => d.Producto)
                .WithMany()
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
