
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using SistemaVenta.Infrastructure.Persistence;

namespace SistemaVenta.Infrastructure.Tests.Persistence;

public class TestDbContextFactory
    : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<TestDbContextFactory>()
            .Build();

        var connectionString =
            configuration.GetConnectionString("TestDatabase");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "No se configuró TestDatabase.");
        }

        var connection = new Npgsql.NpgsqlConnectionStringBuilder(
            connectionString);

        if (connection.Database != "sistema_venta_tests")
        {
            throw new InvalidOperationException(
                "La conexión no apunta a la base de pruebas.");
        }

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString,
                npgsql => npgsql.MigrationsAssembly(
                    "SistemaVenta.Infrastructure"))
            .Options;

        return new AppDbContext(options);
    }
}
