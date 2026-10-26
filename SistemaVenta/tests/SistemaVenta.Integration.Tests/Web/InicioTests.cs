
using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using SistemaVenta.Integration.Tests.Infrastructure;
using Xunit;

namespace SistemaVenta.Integration.Tests.Web;

public class InicioTests
{
    [Fact]
    public async Task UsuarioAnonimo_DebeSerRedirigidoAlLogin()
    {
        await using var aplicacion =
            new SistemaVentaWebApplicationFactory();

        var cliente = aplicacion.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

        var respuesta = await cliente.GetAsync("/Productos");

        Assert.Equal(
            HttpStatusCode.Redirect,
            respuesta.StatusCode);

        Assert.NotNull(respuesta.Headers.Location);

        var destino = respuesta.Headers.Location!;

        Assert.Equal(
            "/Account/Login",
            destino.AbsolutePath);
    }
}
