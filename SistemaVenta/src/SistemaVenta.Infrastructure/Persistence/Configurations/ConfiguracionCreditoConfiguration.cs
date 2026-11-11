
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Infrastructure.Persistence.Configurations;

public class ConfiguracionCreditoConfiguration
    : IEntityTypeConfiguration<ConfiguracionCredito>
{
    public void Configure(
        EntityTypeBuilder<ConfiguracionCredito> builder)
    {
        builder.ToTable("ConfiguracionesCredito");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.LimitePredeterminado)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.HasData(new
        {
            Id = 1,
            LimitePredeterminado = 100000m
        });
    }
}
