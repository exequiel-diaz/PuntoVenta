
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Infrastructure.Persistence;
using SistemaVenta.Infrastructure.Repositories;
using Xunit;
using Npgsql;

namespace SistemaVenta.Infrastructure.Tests.Repositories;

public class ClienteRepositoryTests
{
    private static DbContextOptions<AppDbContext> CrearOpciones()
    {
        var configuracion = new ConfigurationBuilder()
            .AddUserSecrets<ClienteRepositoryTests>()
            .Build();

        var connectionString =
            configuracion.GetConnectionString("TestDatabase");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "No se configuró TestDatabase en secrets.json.");
        }

        var conexion = new Npgsql.NpgsqlConnectionStringBuilder(
            connectionString);

        if (!string.Equals(
            conexion.Database,
            "sistema_venta_tests",
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "La prueba debe utilizar sistema_venta_tests.");
        }

        return new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options;
    }


    [Fact]
    public async Task AgregarClienteFisico_DebePersistirEnPostgreSQL()
    {
        // Arrange
        var opciones = CrearOpciones();

        var email = $"test-{Guid.NewGuid():N}@ejemplo.com";
        var dni = Random.Shared.Next(
            10_000_000,
            100_000_000).ToString();

        await using var contexto = new AppDbContext(opciones);

        await contexto.Database.OpenConnectionAsync();

        await using var transaccion =
            await contexto.Database.BeginTransactionAsync();

        var cliente = new ClienteFisico(
            nombre: "Juan",
            apellido: "Pérez",
            dni: dni,
            telefono: "3811234567",
            email: email,
            direccion: "San Miguel de Tucumán"
        );

        // Act
        var repositorio = new ClienteRepository(contexto);

        await repositorio.AgregarAsync(cliente);
        await contexto.GuardarCambiosAsync();

        int clienteId = cliente.Id;

        // Limpiamos el seguimiento de entidades.
        contexto.ChangeTracker.Clear();

        var recuperado =
            await repositorio.ObtenerPorIdAsync(clienteId);

        // Assert
        var clienteFisico =
            Assert.IsType<ClienteFisico>(recuperado);

        Assert.Equal("Juan", clienteFisico.Nombre);
        Assert.Equal("Pérez", clienteFisico.Apellido);
        Assert.Equal(dni, clienteFisico.Dni);
        Assert.Equal(email, clienteFisico.Email);

        // Deshacemos los cambios realizados por el test.
        await transaccion.RollbackAsync();
    }


    [Fact]
    public async Task AgregarClienteEmpresa_DebePersistirEnPostgreSQL()
    {
        // Arrange
        var opciones = CrearOpciones();

        var email = $"empresa-{Guid.NewGuid():N}@ejemplo.com";
        var cuit = Random.Shared.NextInt64(
            10_000_000_000,
            100_000_000_000).ToString();

        await using var contexto = new AppDbContext(opciones);

        await contexto.Database.OpenConnectionAsync();

        await using var transaccion =
            await contexto.Database.BeginTransactionAsync();

        var cliente = new ClienteEmpresa(
            razonSocial: "Tecnología del Norte S.R.L.",
            cuit: cuit,
            telefono: "3814567890",
            email: email,
            direccion: "San Miguel de Tucumán",
            contacto: "María González"
        );

        // Act
        var repositorio = new ClienteRepository(contexto);

        await repositorio.AgregarAsync(cliente);
        await contexto.GuardarCambiosAsync();

        int clienteId = cliente.Id;

        contexto.ChangeTracker.Clear();

        var recuperado =
            await repositorio.ObtenerPorIdAsync(clienteId);

        // Assert
        var empresa = Assert.IsType<ClienteEmpresa>(recuperado);

        Assert.Equal(
            "Tecnología del Norte S.R.L.",
            empresa.RazonSocial);

        Assert.Equal(cuit, empresa.Cuit);
        Assert.Equal(email, empresa.Email);
        Assert.Equal("María González", empresa.Contacto);

        await transaccion.RollbackAsync();
    }

    [Fact]
    public async Task AgregarClienteFisico_ConDniDuplicado_DebeFallar()
    {
        // Arrange
        var opciones = CrearOpciones();

        await using var contexto = new AppDbContext(opciones);

        await using var transaccion =
            await contexto.Database.BeginTransactionAsync();

        var dni = Random.Shared.Next(10_000_000, 100_000_000).ToString();

        var cliente1 = new ClienteFisico(
            nombre: "Juan",
            apellido: "Pérez",
            dni: dni,
            telefono: "3811111111",
            email: $"juan-{Guid.NewGuid():N}@ejemplo.com",
            direccion: "Tucumán"
        );

        var cliente2 = new ClienteFisico(
            nombre: "Pedro",
            apellido: "Gómez",
            dni: dni,
            telefono: "3812222222",
            email: $"pedro-{Guid.NewGuid():N}@ejemplo.com",
            direccion: "Tucumán"
        );

        var repositorio = new ClienteRepository(contexto);

        // Act: guardamos el primer cliente
        await repositorio.AgregarAsync(cliente1);
        await contexto.GuardarCambiosAsync();

        // Intentamos guardar otro con el mismo DNI
        await repositorio.AgregarAsync(cliente2);

        // Assert: PostgreSQL debe rechazarlo

        var excepcion = await Assert.ThrowsAsync<DbUpdateException>(
            () => contexto.GuardarCambiosAsync());

        var errorPostgres =
            Assert.IsType<PostgresException>(excepcion.InnerException);

        Assert.Equal(
            PostgresErrorCodes.UniqueViolation,
            errorPostgres.SqlState);

        Assert.Equal(
            "IX_Clientes_Dni",
            errorPostgres.ConstraintName);


        // La transacción se revierte al desecharse.
    }



    [Fact]
    public async Task AgregarEmpresa_ConCuitDeClienteFisico_DebeFallar()
    {
        // Arrange
        var opciones = CrearOpciones();

        await using var contexto = new AppDbContext(opciones);
        await using var transaccion =
            await contexto.Database.BeginTransactionAsync();

        var dni = Random.Shared.Next(
            10_000_000,
            100_000_000).ToString();

        var cuit = Random.Shared.NextInt64(
            10_000_000_000,
            100_000_000_000).ToString();

        var fisico = new ClienteFisico(
            nombre: "Juan",
            apellido: "Pérez",
            dni: dni,
            telefono: "3811111111",
            email: $"fisico-{Guid.NewGuid():N}@ejemplo.com",
            direccion: "Tucumán",
            cuit: cuit
        );

        var empresa = new ClienteEmpresa(
            razonSocial: "Empresa de Prueba S.R.L.",
            cuit: cuit,
            telefono: "3812222222",
            email: $"empresa-{Guid.NewGuid():N}@ejemplo.com",
            direccion: "Tucumán"
        );

        var repositorio = new ClienteRepository(contexto);

        // Act
        await repositorio.AgregarAsync(fisico);
        await contexto.GuardarCambiosAsync();

        await repositorio.AgregarAsync(empresa);

        // Assert
        var excepcion = await Assert.ThrowsAsync<DbUpdateException>(
            () => contexto.GuardarCambiosAsync());

        var errorPostgres =
            Assert.IsType<PostgresException>(excepcion.InnerException);

        Assert.Equal(
            PostgresErrorCodes.UniqueViolation,
            errorPostgres.SqlState);

        Assert.Equal(
            "IX_Clientes_Cuit",
            errorPostgres.ConstraintName);
    }



    [Fact]
    public async Task AgregarCliente_ConEmailDuplicado_DebeFallar()
    {
        // Arrange
        var opciones = CrearOpciones();

        await using var contexto = new AppDbContext(opciones);
        await using var transaccion =
            await contexto.Database.BeginTransactionAsync();

        var identificador = Guid.NewGuid().ToString("N");
        var email = $"cliente-{identificador}@ejemplo.com";

        // Documentos válidos para las reglas actuales del dominio.
        var dni = Random.Shared.Next(
            10_000_000,
            100_000_000).ToString();

        var cuit = Random.Shared.NextInt64(
            10_000_000_000,
            100_000_000_000).ToString();

        var cliente1 = new ClienteFisico(
            nombre: "Juan",
            apellido: "Pérez",
            dni: dni,
            telefono: "3811111111",
            email: email,
            direccion: "Tucumán"
        );

        var cliente2 = new ClienteEmpresa(
            razonSocial: "Empresa de Prueba S.R.L.",
            cuit: cuit,
            telefono: "3812222222",
            email: email.ToUpperInvariant(),
            direccion: "Tucumán"
        );

        var repositorio = new ClienteRepository(contexto);

        // Act: guardamos el primer cliente.
        await repositorio.AgregarAsync(cliente1);
        await contexto.GuardarCambiosAsync();

        // Intentamos guardar otro cliente con el mismo email.
        await repositorio.AgregarAsync(cliente2);

        // Assert: PostgreSQL debe rechazar el email duplicado.
        var excepcion = await Assert.ThrowsAsync<DbUpdateException>(
            () => contexto.GuardarCambiosAsync());

        var errorPostgres =
            Assert.IsType<PostgresException>(excepcion.InnerException);

        Assert.Equal(
            PostgresErrorCodes.UniqueViolation,
            errorPostgres.SqlState);

        Assert.Equal(
            "IX_Clientes_Email",
            errorPostgres.ConstraintName);
    }

}
