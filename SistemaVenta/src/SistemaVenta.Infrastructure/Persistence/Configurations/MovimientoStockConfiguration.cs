using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Infrastructure.Persistence.Configurations
{
    internal class MovimientoStockConfiguration
        : IEntityTypeConfiguration<MovimientoStock>
    {
        public void Configure(
            EntityTypeBuilder<MovimientoStock> builder)
        {
            builder.ToTable("MovimientosStock");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.Fecha)
                .IsRequired();

            builder.Property(m => m.Tipo)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(m => m.Cantidad)
                .HasPrecision(18, 3)
                .IsRequired();

            builder.Property(m => m.StockAnterior)
                .HasPrecision(18, 3)
                .IsRequired();

            builder.Property(m => m.StockPosterior)
                .HasPrecision(18, 3)
                .IsRequired();

            builder.Property(m => m.Motivo)
                .HasMaxLength(500);

            builder.HasOne(m => m.Producto)
                .WithMany()
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
