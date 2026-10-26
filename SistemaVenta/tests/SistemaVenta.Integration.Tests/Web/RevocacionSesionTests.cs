
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaVenta.Application.UseCases.Usuarios;
using SistemaVenta.Domain.Enums;
using SistemaVenta.Infrastructure.Persistence;
using SistemaVenta.Integration.Tests.Infrastructure;
using Xunit;
using System.Net;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.Mvc.Testing;

namespace SistemaVenta.Integration.Tests.Web;

public class RevocacionSesionTests
{
    [Fact]
    public async Task RegistrarVendedor_DejaUsuarioEnBaseDePruebas()
    {
        await using var aplicacion =
            new SistemaVentaWebApplicationFactory();

        using var scope =
            aplicacion.Services.CreateScope();

        var servicios = scope.ServiceProvider;

        var registrarUsuario =
            servicios.GetRequiredService<RegistrarUsuario>();

        var contexto =
            servicios.GetRequiredService<AppDbContext>();

        var nombreUsuario =
            $"test_{Guid.NewGuid():N}";

        try
        {
            // Act
            await RegistrarVendedorPruebaAsync(
                 registrarUsuario,
                 nombreUsuario,
                 "PasswordPruebaSegura2026!");

            // Assert
            var usuario = await contexto.Usuarios
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    u => u.NombreUsuario == nombreUsuario);

            Assert.NotNull(usuario);
            Assert.True(usuario.Activo);
            Assert.Equal(RolUsuario.Vendedor, usuario.Rol);
        }
        finally
        {
            await EliminarUsuarioPruebaAsync(
                contexto,
                nombreUsuario);
        }
    }


    [Fact]
    public async Task UsuarioDesactivado_PierdeAccesoConCookieExistente()
    {
        // Arrange
        await using var aplicacion =
            new SistemaVentaWebApplicationFactory();

        using var scope = aplicacion.Services.CreateScope();

        var servicios = scope.ServiceProvider;

        var registrarUsuario =
            servicios.GetRequiredService<RegistrarUsuario>();

        var contexto =
            servicios.GetRequiredService<AppDbContext>();

        var nombreUsuario = $"test_{Guid.NewGuid():N}";
        const string password = "PasswordPruebaSegura2026!";

        var cliente = aplicacion.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                HandleCookies = true,
                BaseAddress = new Uri("https://localhost")
            });

        try
        {
            // 1. Registrar vendedor temporal
            await RegistrarVendedorPruebaAsync(
                registrarUsuario,
                nombreUsuario,
                password);

            // 2. Obtener formulario de Login y token antifalsificación
            var respuestaLogin = await cliente.GetAsync(
                "/Account/Login");

            Assert.Equal(
                HttpStatusCode.OK,
                respuestaLogin.StatusCode);

            var token = await ObtenerTokenLoginAsync(cliente);

            // 3. Enviar credenciales reales
            var respuestaAutenticacion = await IniciarSesionAsync(
                cliente,
                nombreUsuario,
                password);

            Assert.Equal(
                HttpStatusCode.Redirect,
                respuestaAutenticacion.StatusCode);

            Assert.Equal(
                HttpStatusCode.Redirect,
                respuestaAutenticacion.StatusCode);

            // 4. Comprobar que la cookie permite acceder
            var accesoAntes = await cliente.GetAsync("/Productos");

            Assert.Equal(
                HttpStatusCode.OK,
                accesoAntes.StatusCode);

            // 5. Desactivar usuario en PostgreSQL
            await contexto.Usuarios
                .Where(u => u.NombreUsuario == nombreUsuario)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(
                        u => u.Activo, false));

            // 6. Reutilizar la misma cookie
            var accesoDespues = await cliente.GetAsync("/Productos");

            Assert.Equal(
                HttpStatusCode.Redirect,
                accesoDespues.StatusCode);

            Assert.NotNull(accesoDespues.Headers.Location);

            Assert.Equal(
                "/Account/Login",
                accesoDespues.Headers.Location!.AbsolutePath);
        }
        finally
        {
            await EliminarUsuarioPruebaAsync(
                contexto,
                nombreUsuario);
        }
    }

    [Fact]
    public async Task UsuarioDesactivado_NoPuedeIniciarSesion()
    {
        // Arrange
        await using var aplicacion =
            new SistemaVentaWebApplicationFactory();

        using var scope = aplicacion.Services.CreateScope();

        var registrarUsuario = scope.ServiceProvider
            .GetRequiredService<RegistrarUsuario>();

        var contexto = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var nombreUsuario = $"test_{Guid.NewGuid():N}";
        const string password = "PasswordPruebaSegura2026!";

        var cliente = aplicacion.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                HandleCookies = true,
                BaseAddress = new Uri("https://localhost")
            });

        try
        {
            // 1. Registrar un vendedor válido
            await RegistrarVendedorPruebaAsync(
                registrarUsuario,
                nombreUsuario,
                password);

            // 2. Desactivar la cuenta
            await contexto.Usuarios
                .Where(u => u.NombreUsuario == nombreUsuario)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(
                        u => u.Activo, false));

            // 3. Intentar iniciar sesión con la contraseña correcta
            var respuesta = await IniciarSesionAsync(
                cliente,
                nombreUsuario,
                password);

            // 4. El Login debe rechazarse y mostrar el formulario
            Assert.Equal(
                HttpStatusCode.OK,
                respuesta.StatusCode);

            var contenido = await respuesta.Content
                .ReadAsStringAsync();

            Assert.Contains(
                "type=\"password\"",
                contenido,
                StringComparison.OrdinalIgnoreCase);

            // 5. No debe existir una sesión autenticada
            var acceso = await cliente.GetAsync("/Productos");

            Assert.Equal(
                HttpStatusCode.Redirect,
                acceso.StatusCode);

            Assert.NotNull(acceso.Headers.Location);

            Assert.Equal(
                "/Account/Login",
                acceso.Headers.Location!.AbsolutePath);
        }
        finally
        {
            await EliminarUsuarioPruebaAsync(
                contexto,
                nombreUsuario);
        }
    }

    private static async Task RegistrarVendedorPruebaAsync(
        RegistrarUsuario registrarUsuario,
        string nombreUsuario,
        string password)
    {
        await registrarUsuario.EjecutarAsync(
            nombreUsuario,
            "Vendedor de integración",
            password,
            RolUsuario.Vendedor);
    }


    private static async Task<string> ObtenerTokenLoginAsync(
        HttpClient cliente)
    {
        var respuesta = await cliente.GetAsync("/Account/Login");

        Assert.Equal(
            HttpStatusCode.OK,
            respuesta.StatusCode);

        var html = await respuesta.Content.ReadAsStringAsync();

        var parser = new HtmlParser();
        var documento = await parser.ParseDocumentAsync(html);

        var token = documento
            .QuerySelector("input[name='__RequestVerificationToken']")
            ?.GetAttribute("value");

        Assert.False(string.IsNullOrWhiteSpace(token));

        return token!;
    }


    private static async Task<HttpResponseMessage> IniciarSesionAsync(
        HttpClient cliente,
        string nombreUsuario,
        string password)
    {
        // Obtener el token antifalsificación
        var token = await ObtenerTokenLoginAsync(cliente);

        // Preparar los datos del formulario
        var formulario = new Dictionary<string, string>
        {
            ["NombreUsuario"] = nombreUsuario,
            ["Password"] = password,
            ["__RequestVerificationToken"] = token
        };

        // Enviar las credenciales al Login real
        return await cliente.PostAsync(
            "/Account/Login",
            new FormUrlEncodedContent(formulario));
    }


    private static async Task EliminarUsuarioPruebaAsync(
        AppDbContext contexto,
        string nombreUsuario)
    {
        if (!nombreUsuario.StartsWith(
            "test_",
            StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Solo se permite eliminar usuarios temporales de prueba.");
        }

        await contexto.Usuarios
            .Where(u => u.NombreUsuario == nombreUsuario)
            .ExecuteDeleteAsync();
    }


}
