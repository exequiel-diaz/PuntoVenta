
using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using SistemaVenta.Integration.Tests.Infrastructure;
using Xunit;

namespace SistemaVenta.Integration.Tests.Web;

public class LoginRealTests
{
    [Fact]
    public async Task Login_DebeEstarDisponibleSinAutenticacion()
    {
        // Arrange
        await using var aplicacion =
            new SistemaVentaWebApplicationFactory();

        var cliente = aplicacion.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                BaseAddress = new Uri("https://localhost")
            });

        // Act
        var respuesta = await cliente.GetAsync(
            "/Account/Login");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            respuesta.StatusCode);

        var contenido =
            await respuesta.Content.ReadAsStringAsync();

        Assert.Contains(
            "password",
            contenido,
            StringComparison.OrdinalIgnoreCase);
    }
}
