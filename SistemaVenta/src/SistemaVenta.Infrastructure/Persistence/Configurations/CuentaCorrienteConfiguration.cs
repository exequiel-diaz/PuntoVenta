using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Infrastructure.Persistence.Configurations;

public class CuentaCorrienteConfiguration
    : IEntityTypeConfiguration<CuentaCorriente>
{
    public void Configure(EntityTypeBuilder<CuentaCorriente> builder)
    {
        builder.ToTable("CuentasCorrientes");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.LimiteDeuda)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(c => c.FechaApertura)
            .IsRequired();

        builder.Property(c => c.Estado)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(c => c.Observaciones)
            .HasMaxLength(500);

        builder.HasIndex(c => c.ClienteId)
            .IsUnique();

        builder.HasOne<Cliente>()
            .WithOne()
            .HasForeignKey<CuentaCorriente>(c => c.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(c => c.SaldoActual);

        builder.HasMany(c => c.Movimientos)
            .WithOne()
            .HasForeignKey(m => m.CuentaCorrienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(c => c.Movimientos)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(c => c.Movimientos)
            .HasField("_movimientos");
    }
}