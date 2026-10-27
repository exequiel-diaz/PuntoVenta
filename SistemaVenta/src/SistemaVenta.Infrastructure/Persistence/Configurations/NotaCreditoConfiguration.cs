
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Infrastructure.Persistence.Configurations
{
    public class NotaCreditoConfiguration
        : IEntityTypeConfiguration<NotaCredito>
    {
        public void Configure(EntityTypeBuilder<NotaCredito> builder)
        {
            builder.Property(n => n.Motivo)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(f => f.ImporteTotal)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(n => n.FacturaId)
                .IsRequired();

            builder.HasOne<Factura>()
                .WithMany()
                .HasForeignKey(n => n.FacturaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
