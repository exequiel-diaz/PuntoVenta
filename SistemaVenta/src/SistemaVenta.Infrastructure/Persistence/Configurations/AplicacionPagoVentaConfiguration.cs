using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Infrastructure.Persistence.Configurations;

public class AplicacionPagoVentaConfiguration
    : IEntityTypeConfiguration<AplicacionPagoVenta>
{
    public void Configure(
        EntityTypeBuilder<AplicacionPagoVenta> builder)
    {
        builder.ToTable("AplicacionesPagoVenta");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.ImporteAplicado)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(a => a.PagoId)
            .IsRequired();

        builder.Property(a => a.VentaId)
            .IsRequired();

        builder.HasOne(a => a.Venta)
            .WithMany()
            .HasForeignKey(a => a.VentaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.VentaId);

        // La relación con Pago se configura en PagoConfiguration.
    }
}