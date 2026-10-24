
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Infrastructure.Persistence.Configurations
{
    public class VentaConfiguration : IEntityTypeConfiguration<Venta>
    {
        public void Configure(EntityTypeBuilder<Venta> builder)
        {
            builder.ToTable("Ventas");

            builder.HasKey(v => v.Id);

            // Control de concurrencia optimista de PostgreSQL.
            // Evita que dos operaciones modifiquen la misma venta
            // utilizando una versión desactualizada.
            builder.Property<uint>("xmin")
                .IsRowVersion();

            builder.Property(v => v.ClienteId)
                .IsRequired();

            builder.Property(v => v.Fecha)
                .IsRequired();

            builder.Property(v => v.Estado)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            // El total se calcula a partir de los detalles.
            builder.Ignore(v => v.Total);

            // Relación con Cliente.
            builder.HasOne<Cliente>()
                .WithMany()
                .HasForeignKey(v => v.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación Venta -> Detalles.
            builder.HasMany(v => v.Detalles)
                .WithOne()
                .HasForeignKey(d => d.VentaId)
                .OnDelete(DeleteBehavior.Cascade);

            // EF Core utiliza el campo privado para
            // mantener encapsulada la colección.
            builder.Navigation(v => v.Detalles)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
