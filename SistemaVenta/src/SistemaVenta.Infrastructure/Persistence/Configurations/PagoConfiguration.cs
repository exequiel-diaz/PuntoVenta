
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Infrastructure.Persistence.Configurations
{
    public class PagoConfiguration : IEntityTypeConfiguration<Pago>
    {
        public void Configure(EntityTypeBuilder<Pago> builder)
        {
            builder.ToTable("Pagos");

            builder.HasKey(p => p.Id);

            // Control de concurrencia optimista.
            builder.Property<uint>("xmin")
                .IsRowVersion();

            builder.Property(p => p.Fecha)
                .IsRequired();

            builder.Property(p => p.Importe)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(p => p.FormaPago)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(p => p.Estado)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(p => p.Observaciones)
                .HasMaxLength(500);

            builder.Property(p => p.UsuarioId)
                .IsRequired();

            // El importe disponible se calcula
            // a partir de las aplicaciones.
            builder.Ignore(p => p.ImporteDisponible);

            // Relación Pago -> AplicacionesPago.
            builder.HasMany(p => p.Aplicaciones)
                .WithOne(a => a.Pago)
                .HasForeignKey(a => a.PagoId)
                .OnDelete(DeleteBehavior.Restrict);

            // EF Core utiliza la colección privada.
            builder.Navigation(p => p.Aplicaciones)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            // Relación con el usuario responsable.
            builder.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(p => p.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación Pago -> AplicacionesPagoVenta.
            builder.HasMany(p => p.AplicacionesVentas)
                .WithOne(a => a.Pago)
                .HasForeignKey(a => a.PagoId)
                .OnDelete(DeleteBehavior.Restrict);

            // EF Core utiliza la colección privada.
            builder.Navigation(p => p.AplicacionesVentas)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
