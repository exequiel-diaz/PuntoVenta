using Microsoft.EntityFrameworkCore;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;
using SistemaVenta.Infrastructure.Repositories;
using SistemaVenta.Infrastructure.Tests.Persistence;
using Xunit;

namespace SistemaVenta.Infrastructure.Tests.Repositories;

public class CuentaCorrienteRepositoryTests
{
    private readonly TestDbContextFactory _factory = new();

    [Fact]
    public async Task AgregarCuenta_DebePersistirYGenerarId()
    {
        // Arrange
        await using var context = _factory.CreateDbContext([]);

        await using var transaccion =
            await context.Database.BeginTransactionAsync();

        var documento = Random.Shared.Next(10000000, 99999999)
            .ToString();

        var cliente = new ClienteFisico(
            nombre: "Cliente",
            apellido: "Prueba",
            dni: documento,
            telefono: "3811234567",
            email: $"prueba{Guid.NewGuid():N}@test.com",
            direccion: "Tucumán"
        );

        context.Clientes.Add(cliente);
        await context.SaveChangesAsync();

        var cuenta = new CuentaCorriente(
            clienteId: cliente.Id,
            limiteDeuda: 500000m,
            observaciones: "Prueba de integración"
        );

        var repositorio =
            new CuentaCorrienteRepository(context);

        // Act
        await repositorio.AgregarAsync(cuenta);
        await context.GuardarCambiosAsync();

        // Assert: PostgreSQL generó el identificador
        Assert.True(cuenta.Id > 0);

        // Limpiamos el seguimiento para forzar una lectura real.
        context.ChangeTracker.Clear();

        var cuentaRecuperada =
            await repositorio.ObtenerPorIdAsync(cuenta.Id);

        Assert.NotNull(cuentaRecuperada);
        Assert.Equal(cliente.Id, cuentaRecuperada.ClienteId);
        Assert.Equal(500000m, cuentaRecuperada.LimiteDeuda);
        Assert.Equal(EstadoCuentaCorriente.Activa, cuentaRecuperada.Estado);
        Assert.Equal(0m, cuentaRecuperada.SaldoActual);

    }


    [Fact]
    public async Task GuardarMovimientos_DebeRecuperarSaldoCorrectamente()
    {
        // Arrange
        await using var context = _factory.CreateDbContext([]);

        await using var transaccion =
            await context.Database.BeginTransactionAsync();

        var dni = Random.Shared.Next(10000000, 99999999)
            .ToString();

        var cliente = new ClienteFisico(
            nombre: "Cliente",
            apellido: "Movimientos",
            dni: dni,
            telefono: "3811234567",
            email: $"movimientos{Guid.NewGuid():N}@test.com",
            direccion: "Tucumán"
        );

        context.Clientes.Add(cliente);
        await context.SaveChangesAsync();

        var cuenta = new CuentaCorriente(
            clienteId: cliente.Id,
            limiteDeuda: 500000m
        );

        var repositorio = new CuentaCorrienteRepository(context);

        await repositorio.AgregarAsync(cuenta);
        await context.GuardarCambiosAsync();

        int cuentaId = cuenta.Id;

        // Act: registrar una deuda y un pago
        cuenta.RegistrarDebito(
            200000m,
            "Venta de notebook");

        cuenta.RegistrarCredito(
            50000m,
            "Pago parcial");

        await context.GuardarCambiosAsync();

        // Quitamos las entidades del seguimiento de EF Core
        // para obligarlo a recuperar los datos de PostgreSQL.
        context.ChangeTracker.Clear();

        var cuentaRecuperada =
            await repositorio.ObtenerPorIdAsync(cuentaId);

        // Assert
        Assert.NotNull(cuentaRecuperada);

        Assert.Equal(2, cuentaRecuperada.Movimientos.Count);

        Assert.Equal(150000m, cuentaRecuperada.SaldoActual);

        Assert.Contains(
            cuentaRecuperada.Movimientos,
            m => m.EsDebito() && m.Importe == 200000m);

        Assert.Contains(
            cuentaRecuperada.Movimientos,
            m => m.EsCredito() && m.Importe == 50000m);

        
    }


    [Fact]
    public async Task ModificarCuenta_DebePersistirLimiteYEstado()
    {
        // Arrange
        await using var context = _factory.CreateDbContext([]);

        await using var transaccion =
            await context.Database.BeginTransactionAsync();

        var cliente = new ClienteFisico(
            nombre: "Cliente",
            apellido: "Actualizacion",
            dni: Random.Shared.Next(10000000, 99999999).ToString(),
            telefono: "3811234567",
            email: $"actualizacion{Guid.NewGuid():N}@test.com",
            direccion: "Tucumán"
        );

        context.Clientes.Add(cliente);
        await context.SaveChangesAsync();

        var cuenta = new CuentaCorriente(
            clienteId: cliente.Id,
            limiteDeuda: 500000m);

        var repositorio = new CuentaCorrienteRepository(context);

        await repositorio.AgregarAsync(cuenta);
        await context.GuardarCambiosAsync();

        int cuentaId = cuenta.Id;

        // Act
        cuenta.ModificarLimiteDeuda(300000m);
        cuenta.Suspender();

        await context.GuardarCambiosAsync();

        context.ChangeTracker.Clear();

        var cuentaRecuperada =
            await repositorio.ObtenerPorIdAsync(cuentaId);

        // Assert
        Assert.NotNull(cuentaRecuperada);

        Assert.Equal(300000m, cuentaRecuperada.LimiteDeuda);

        Assert.Equal(
            EstadoCuentaCorriente.Suspendida,
            cuentaRecuperada.Estado);

        // La transacción se revierte al finalizar.
    }



    [Fact]
    public async Task AgregarCuentaDuplicada_DebeFallarPorRestriccionUnica()
    {
        // Arrange
        await using var context = _factory.CreateDbContext([]);

        await using var transaccion =
            await context.Database.BeginTransactionAsync();

        var cliente = new ClienteFisico(
            nombre: "Cliente",
            apellido: "Duplicado",
            dni: Random.Shared.Next(10000000, 99999999).ToString(),
            telefono: "3811234567",
            email: $"duplicado{Guid.NewGuid():N}@test.com",
            direccion: "Tucumán"
        );

        context.Clientes.Add(cliente);
        await context.SaveChangesAsync();

        int clienteId = cliente.Id;

        var repositorio = new CuentaCorrienteRepository(context);

        var primeraCuenta = new CuentaCorriente(
            clienteId: clienteId,
            limiteDeuda: 200000m);

        await repositorio.AgregarAsync(primeraCuenta);
        await context.GuardarCambiosAsync();

        // Importante:
        // Dejamos de seguir las entidades guardadas.
        // Los datos permanecen en PostgreSQL dentro
        // de la transacción.
        context.ChangeTracker.Clear();

        // Act: intentamos crear una segunda cuenta
        // para el mismo cliente.
        var segundaCuenta = new CuentaCorriente(
            clienteId: clienteId,
            limiteDeuda: 300000m);

        await repositorio.AgregarAsync(segundaCuenta);

        var excepcion =
            await Assert.ThrowsAsync<DbUpdateException>(
                () => context.GuardarCambiosAsync());

        // Assert
        var errorPostgres =
            Assert.IsType<Npgsql.PostgresException>(
                excepcion.InnerException);

        Assert.Equal(
            Npgsql.PostgresErrorCodes.UniqueViolation,
            errorPostgres.SqlState);

        // Sin Commit: se revierten los datos.
    }


}