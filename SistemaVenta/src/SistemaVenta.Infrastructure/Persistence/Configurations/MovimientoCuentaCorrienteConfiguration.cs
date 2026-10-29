using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Infrastructure.Persistence.Configurations;

public class MovimientoCuentaCorrienteConfiguration
    : IEntityTypeConfiguration<MovimientoCuentaCorriente>
{
    public void Configure(
        EntityTypeBuilder<MovimientoCuentaCorriente> builder)
    {
        builder.ToTable("MovimientosCuentaCorriente");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Fecha)
            .IsRequired();

        builder.Property(m => m.Tipo)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(m => m.Importe)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(m => m.Descripcion)
            .HasMaxLength(500)
            .IsRequired();

        builder.HasIndex(m => m.CuentaCorrienteId);

        builder.HasOne(m => m.Venta)
            .WithMany()
            .HasForeignKey(m => m.VentaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => m.VentaId);

        // Relación opcional con el pago que originó el movimiento.
        builder.HasOne(m => m.Pago)
            .WithMany()
            .HasForeignKey(m => m.PagoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índice para consultar movimientos asociados a un pago.
        builder.HasIndex(m => m.PagoId);


    }
}