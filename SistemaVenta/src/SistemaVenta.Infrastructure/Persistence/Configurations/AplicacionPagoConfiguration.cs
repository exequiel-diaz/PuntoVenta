
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Infrastructure.Persistence.Configurations
{
    public class AplicacionPagoConfiguration
        : IEntityTypeConfiguration<AplicacionPago>
    {
        public void Configure(
            EntityTypeBuilder<AplicacionPago> builder)
        {
            builder.ToTable("AplicacionesPago");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.ImporteAplicado)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(a => a.PagoId)
                .IsRequired();

            builder.Property(a => a.DocumentoComercialId)
                .IsRequired();


            builder.HasOne<DocumentoComercial>()
                .WithMany()
                .HasForeignKey(a => a.DocumentoComercialId)
                .OnDelete(DeleteBehavior.Restrict);


            // La relación con Pago ya está definida
            // en PagoConfiguration.

            // La relación con DocumentoComercial
            // se configurará cuando definamos
            // el mapeo de esa jerarquía.
        }
    }
}
