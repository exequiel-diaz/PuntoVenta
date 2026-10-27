
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Infrastructure.Persistence.Configurations
{
    public class TicketConfiguration
        : IEntityTypeConfiguration<Ticket>
    {
        public void Configure(EntityTypeBuilder<Ticket> builder)
        {
            builder.Property(t => t.ImporteTotal)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(t => t.Tipo)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            // Una cotización puede no tener una venta asociada.
            builder.Property(t => t.VentaId)
                .IsRequired(false);

            builder.HasOne<Venta>()
                .WithMany()
                .HasForeignKey(t => t.VentaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
