
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Infrastructure.Persistence.Configurations
{
    public class ReciboConfiguration
        : IEntityTypeConfiguration<Recibo>
    {
        public void Configure(EntityTypeBuilder<Recibo> builder)
        {
            builder.Property(r => r.ImporteTotal)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(r => r.PagoId)
                .IsRequired();

            builder.HasOne<Pago>()
                .WithMany()
                .HasForeignKey(r => r.PagoId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
