
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Infrastructure.Persistence.Configurations;

public class PermisoUsuarioConfiguration
    : IEntityTypeConfiguration<PermisoUsuario>
{
    public void Configure(
        EntityTypeBuilder<PermisoUsuario> builder)
    {
        builder.ToTable("permisos_usuario");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.CodigoPermiso)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(p => new
        {
            p.UsuarioId,
            p.CodigoPermiso
        }).IsUnique();

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(p => p.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
