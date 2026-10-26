
using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using SistemaVenta.Integration.Tests.Infrastructure;
using Xunit;

namespace SistemaVenta.Integration.Tests.Web;

public class ProductosAutorizacionTests
{
    [Fact]
    public async Task Vendedor_NoPuedeAccederACrearProducto()
    {
        // Arrange
        await using var aplicacion =
            new SistemaVentaAuthorizationFactory();

        var cliente = aplicacion.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                BaseAddress =
                    new Uri("https://localhost")
            });

        cliente.DefaultRequestHeaders.Add(
            "X-Test-Role",
            "Vendedor");

        // Act
        var respuesta = await cliente.GetAsync(
            "/Productos/Crear");

        // Assert
        Assert.Equal(
            HttpStatusCode.Forbidden,
            respuesta.StatusCode);
    }


    [Fact]
    public async Task Administrador_PuedeAccederACrearProducto()
    {
        // Arrange
        await using var aplicacion =
            new SistemaVentaAuthorizationFactory();

        var cliente = aplicacion.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                BaseAddress = new Uri("https://localhost")
            });

        cliente.DefaultRequestHeaders.Add(
            "X-Test-Role",
            "Administrador");

        // Act
        var respuesta = await cliente.GetAsync(
            "/Productos/Crear");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            respuesta.StatusCode);
    }


    [Fact]
    public async Task Vendedor_PuedeAccederAlListadoProductos()
    {
        // Arrange
        await using var aplicacion =
            new SistemaVentaAuthorizationFactory();

        var cliente = aplicacion.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                BaseAddress = new Uri("https://localhost")
            });

        cliente.DefaultRequestHeaders.Add(
            "X-Test-Role",
            "Vendedor");

        // Act
        var respuesta = await cliente.GetAsync(
            "/Productos");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            respuesta.StatusCode);
    }


}
