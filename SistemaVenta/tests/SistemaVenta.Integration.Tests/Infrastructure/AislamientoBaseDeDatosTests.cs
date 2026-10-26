
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaVenta.Infrastructure.Persistence;
using Xunit;

namespace SistemaVenta.Integration.Tests.Infrastructure;

public class AislamientoBaseDatosTests
{
    [Fact]
    public async Task AppDbContext_DebeUtilizarBaseDePruebas()
    {
        // Arrange
        await using var aplicacion =
            new SistemaVentaWebApplicationFactory();

        using var scope =
            aplicacion.Services.CreateScope();

        var contexto = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        // Act
        var conexion = contexto.Database.GetDbConnection();

        // Assert
        Assert.Equal(
            "sistema_venta_tests",
            conexion.Database);

        Assert.True(
            await contexto.Database.CanConnectAsync());
    }
}
