
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using SistemaVenta.Infrastructure.Persistence;

namespace SistemaVenta.Integration.Tests.Infrastructure;

public class SistemaVentaWebApplicationFactory
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        var configuracion = new ConfigurationBuilder()
            .AddUserSecrets<SistemaVentaWebApplicationFactory>()
            .Build();

        var conexionPruebas = configuracion
            .GetConnectionString("TestConnection");

        if (string.IsNullOrWhiteSpace(conexionPruebas))
        {
            throw new InvalidOperationException(
                "No se configuró TestConnection.");
        }

        var datosConexion =
            new NpgsqlConnectionStringBuilder(conexionPruebas);

        if (!string.Equals(
            datosConexion.Database,
            "sistema_venta_tests",
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Las pruebas deben utilizar sistema_venta_tests.");
        }

        // Evita que la aplicación conserve su conexión normal.
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] =
                        conexionPruebas
                });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(conexionPruebas));
        });
    }
}
