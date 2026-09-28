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
    public class ProductoConfiguration
        : IEntityTypeConfiguration<Producto>
    {
        public void Configure(
            EntityTypeBuilder<Producto> builder)
        {
            builder.ToTable("Productos");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.CodigoInterno)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(p => p.CodigoInterno)
                .IsUnique();

            builder.Property(p => p.Nombre)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(p => p.Descripcion)
                .HasMaxLength(500);

            builder.Property(p => p.PrecioVenta)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(p => p.Costo)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(p => p.StockActual)
                .HasPrecision(18, 3)
                .IsRequired();

            builder.Property(p => p.StockMinimo)
                .HasPrecision(18, 3)
                .IsRequired();

            builder.Property(p => p.Estado)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(p => p.FechaAlta)
                .IsRequired();

            builder.HasOne(p => p.Categoria)
                .WithMany()
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
