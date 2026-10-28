
using Microsoft.Extensions.DependencyInjection;
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Integration.Tests.Infrastructure;
using Xunit;

namespace SistemaVenta.Integration.Tests.Pagos;

public class DocumentoComercialRepositoryTests
{
    [Fact]
    public async Task ObtenerPorId_DocumentoInexistente_DebeRetornarNull()
    {
        // Arrange
        await using var aplicacion =
            new SistemaVentaWebApplicationFactory();

        using var scope =
            aplicacion.Services.CreateScope();

        var repositorio = scope.ServiceProvider
            .GetRequiredService<IDocumentoComercialRepository>();

        // Act
        var documento =
            await repositorio.ObtenerPorIdAsync(-1);

        // Assert
        Assert.Null(documento);
    }

    [Fact]
    public async Task ObtenerTotalAplicado_DocumentoSinAplicaciones_DebeRetornarCero()
    {
        // Arrange
        await using var aplicacion =
            new SistemaVentaWebApplicationFactory();

        using var scope =
            aplicacion.Services.CreateScope();

        var repositorio = scope.ServiceProvider
            .GetRequiredService<IDocumentoComercialRepository>();

        // Act
        var totalAplicado =
            await repositorio.ObtenerTotalAplicadoAsync(-1);

        // Assert
        Assert.Equal(0m, totalAplicado);
    }
}
