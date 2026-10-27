
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Infrastructure.Persistence.Configurations
{
    public class NotaDebitoConfiguration
        : IEntityTypeConfiguration<NotaDebito>
    {
        public void Configure(EntityTypeBuilder<NotaDebito> builder)
        {
            builder.Property(n => n.Motivo)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(n => n.ImporteTotal)
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
