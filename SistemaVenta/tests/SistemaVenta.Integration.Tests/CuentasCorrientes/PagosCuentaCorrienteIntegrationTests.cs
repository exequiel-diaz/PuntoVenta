using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;
using SistemaVenta.Infrastructure.Persistence;
using SistemaVenta.Integration.Tests.Infrastructure;
using Xunit;

namespace SistemaVenta.Integration.Tests.CuentasCorrientes;

public class PagosCuentaCorrienteIntegrationTests
{
    [Fact]
    public async Task RegistrarPagoDeVenta_DebeReducirSaldoPendiente()
    {
        // Arrange
        await using var aplicacion =
            new SistemaVentaWebApplicationFactory();

        using var scope =
            aplicacion.Services.CreateScope();

        var contexto = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        await using var transaccion =
            await contexto.Database.BeginTransactionAsync();

        var identificador = Guid.NewGuid().ToString("N");

        // 1. Crear cliente.
        var cliente = new ClienteFisico(
            "Cliente",
            "Prueba",
            Random.Shared.Next(10_000_000, 100_000_000).ToString(),
            "3815551234",
            $"cliente_{identificador}@test.com",
            "Dirección de prueba");

        contexto.Clientes.Add(cliente);
        await contexto.SaveChangesAsync();

        // 2. Crear venta.
        var venta = new Venta(cliente.Id);

        contexto.Ventas.Add(venta);
        await contexto.SaveChangesAsync();

        // 3. Crear cuenta corriente.
        var cuenta = new CuentaCorriente(
            cliente.Id,
            100000m);

        cuenta.RegistrarDebito(
            50000m,
            "Venta de prueba",
            venta);

        contexto.CuentasCorrientes.Add(cuenta);
        await contexto.SaveChangesAsync();

        // 4. Crear usuario.
        var usuario = new Usuario(
            $"test_{identificador[..20]}",
            "Vendedor de prueba",
            "hash-de-prueba-no-utilizado-para-login",
            RolUsuario.Vendedor);

        contexto.Usuarios.Add(usuario);
        await contexto.SaveChangesAsync();

        // 5. Crear y guardar pago.
        var pago = new Pago(
            20000m,
            FormaPago.Efectivo,
            usuario.Id);

        contexto.Pagos.Add(pago);
        await contexto.SaveChangesAsync();

        // Act
        cuenta.RegistrarPagoDeVenta(
            venta,
            pago,
            20000m);

        await contexto.SaveChangesAsync();

        var cuentaId = cuenta.Id;

        // Limpiar el seguimiento de EF Core.
        contexto.ChangeTracker.Clear();

        // Recuperar movimientos desde PostgreSQL.
        var cuentaRecuperada = await contexto.CuentasCorrientes
            .Include(c => c.Movimientos)
            .SingleAsync(c => c.Id == cuentaId);

        // Assert
        Assert.Equal(
            30000m,
            cuentaRecuperada.SaldoActual);

        Assert.Equal(
            30000m,
            cuentaRecuperada.ObtenerSaldoPendienteDeVenta(venta.Id));

        var movimientoPago = Assert.Single(
            cuentaRecuperada.Movimientos.Where(m =>
                m.EsCredito() &&
                m.PagoId == pago.Id));

        Assert.Equal(20000m, movimientoPago.Importe);
        Assert.Equal(venta.Id, movimientoPago.VentaId);
        Assert.Equal(pago.Id, movimientoPago.PagoId);

        // Sin Commit: los datos se revierten al finalizar.
    }

    [Fact]
    public async Task RegistrarPagoDeVenta_NoDebePermitirSuperarSaldoPendiente()
    {
        // Arrange
        await using var aplicacion =
            new SistemaVentaWebApplicationFactory();

        using var scope =
            aplicacion.Services.CreateScope();

        var contexto = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        await using var transaccion =
            await contexto.Database.BeginTransactionAsync();

        var identificador = Guid.NewGuid().ToString("N");

        var cliente = new ClienteFisico(
            "Cliente",
            "Prueba",
            Random.Shared.Next(10_000_000, 100_000_000).ToString(),
            "3815551234",
            $"cliente_{identificador}@test.com",
            "Dirección de prueba");

        contexto.Clientes.Add(cliente);
        await contexto.SaveChangesAsync();

        var venta = new Venta(cliente.Id);

        contexto.Ventas.Add(venta);
        await contexto.SaveChangesAsync();

        var cuenta = new CuentaCorriente(
            cliente.Id,
            100000m);

        cuenta.RegistrarDebito(
            50000m,
            "Venta de prueba",
            venta);

        contexto.CuentasCorrientes.Add(cuenta);
        await contexto.SaveChangesAsync();

        var usuario = new Usuario(
            $"test_{identificador[..20]}",
            "Vendedor de prueba",
            "hash-de-prueba-no-utilizado-para-login",
            RolUsuario.Vendedor);

        contexto.Usuarios.Add(usuario);
        await contexto.SaveChangesAsync();

        // Primer pago: $30.000.
        var primerPago = new Pago(
            30000m,
            FormaPago.Efectivo,
            usuario.Id);

        // Segundo pago: $25.000.
        var segundoPago = new Pago(
            25000m,
            FormaPago.Efectivo,
            usuario.Id);

        contexto.Pagos.AddRange(primerPago, segundoPago);
        await contexto.SaveChangesAsync();

        cuenta.RegistrarPagoDeVenta(
            venta,
            primerPago,
            30000m);

        await contexto.SaveChangesAsync();

        // Act: intentamos pagar $25.000,
        // aunque solamente quedan $20.000.
        var excepcion = Assert.Throws<InvalidOperationException>(
            () => cuenta.RegistrarPagoDeVenta(
                venta,
                segundoPago,
                25000m));

        // Assert
        Assert.Contains(
            "saldo pendiente",
            excepcion.Message,
            StringComparison.OrdinalIgnoreCase);

        Assert.Equal(
            20000m,
            cuenta.SaldoActual);

        Assert.Equal(
            20000m,
            cuenta.ObtenerSaldoPendienteDeVenta(venta.Id));

        // El intento rechazado no debe crear otro crédito.
        Assert.Single(
            cuenta.Movimientos.Where(m => m.EsCredito()));

        Assert.DoesNotContain(
            cuenta.Movimientos,
            m => m.PagoId == segundoPago.Id);

        // La transacción se revierte al finalizar.
    }

    [Fact]
    public async Task RevertirDebitoDeVenta_NoDebePermitirAnularVentaConPagos()
    {
        // Arrange
        await using var aplicacion =
            new SistemaVentaWebApplicationFactory();

        using var scope =
            aplicacion.Services.CreateScope();

        var contexto = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        await using var transaccion =
            await contexto.Database.BeginTransactionAsync();

        var identificador = Guid.NewGuid().ToString("N");

        // Crear cliente.
        var cliente = new ClienteFisico(
            "Cliente",
            "Prueba",
            Random.Shared.Next(10_000_000, 100_000_000).ToString(),
            "3815551234",
            $"cliente_{identificador}@test.com",
            "Dirección de prueba");

        contexto.Clientes.Add(cliente);
        await contexto.SaveChangesAsync();

        // Crear dos ventas del mismo cliente.
        var ventaComputadora = new Venta(cliente.Id);
        var ventaTeclado = new Venta(cliente.Id);

        contexto.Ventas.AddRange(
            ventaComputadora,
            ventaTeclado);

        await contexto.SaveChangesAsync();

        // Crear cuenta corriente.
        var cuenta = new CuentaCorriente(
            cliente.Id,
            200000m);

        cuenta.RegistrarDebito(
            100000m,
            "Venta de computadora",
            ventaComputadora);

        cuenta.RegistrarDebito(
            70000m,
            "Venta de teclado",
            ventaTeclado);

        contexto.CuentasCorrientes.Add(cuenta);
        await contexto.SaveChangesAsync();

        // Crear usuario.
        var usuario = new Usuario(
            $"test_{identificador[..20]}",
            "Vendedor de prueba",
            "hash-de-prueba-no-utilizado-para-login",
            RolUsuario.Vendedor);

        contexto.Usuarios.Add(usuario);
        await contexto.SaveChangesAsync();

        // Registrar un pago parcial de la computadora.
        var pago = new Pago(
            30000m,
            FormaPago.Efectivo,
            usuario.Id);

        contexto.Pagos.Add(pago);
        await contexto.SaveChangesAsync();

        cuenta.RegistrarPagoDeVenta(
            ventaComputadora,
            pago,
            30000m);

        await contexto.SaveChangesAsync();

        // Comprobamos que la deuda total es suficiente
        // para compensar los $100.000 de la computadora.
        Assert.Equal(140000m, cuenta.SaldoActual);

        // Act: intentamos anular la venta pagada parcialmente.
        var excepcion = Assert.Throws<InvalidOperationException>(
            () => cuenta.RevertirDebitoDeVenta(
                ventaComputadora));

        // Assert
        Assert.Contains(
            "pagos aplicados",
            excepcion.Message,
            StringComparison.OrdinalIgnoreCase);

        // La operación rechazada no debe alterar el saldo.
        Assert.Equal(140000m, cuenta.SaldoActual);

        Assert.Equal(
            70000m,
            cuenta.ObtenerSaldoPendienteDeVenta(
                ventaComputadora.Id));

        // Deben existir dos débitos y un solo crédito.
        Assert.Equal(
            2,
            cuenta.Movimientos.Count(m => m.EsDebito()));

        Assert.Single(
            cuenta.Movimientos.Where(m => m.EsCredito()));

        // Sin Commit: se revierten los datos de prueba.
    }

    [Fact]
    public async Task RegistrarPagoDeVenta_NoDebePermitirPagarVentaConDebitoRevertido()
    {
        // Arrange
        await using var aplicacion =
            new SistemaVentaWebApplicationFactory();

        using var scope =
            aplicacion.Services.CreateScope();

        var contexto = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        await using var transaccion =
            await contexto.Database.BeginTransactionAsync();

        var identificador = Guid.NewGuid().ToString("N");

        var cliente = new ClienteFisico(
            "Cliente",
            "Prueba",
            Random.Shared.Next(10_000_000, 100_000_000).ToString(),
            "3815551234",
            $"cliente_{identificador}@test.com",
            "Dirección de prueba");

        contexto.Clientes.Add(cliente);
        await contexto.SaveChangesAsync();

        var ventaA = new Venta(cliente.Id);
        var ventaB = new Venta(cliente.Id);

        contexto.Ventas.AddRange(ventaA, ventaB);
        await contexto.SaveChangesAsync();

        var cuenta = new CuentaCorriente(
            cliente.Id,
            200000m);

        cuenta.RegistrarDebito(
            50000m,
            "Venta A",
            ventaA);

        cuenta.RegistrarDebito(
            70000m,
            "Venta B",
            ventaB);

        contexto.CuentasCorrientes.Add(cuenta);
        await contexto.SaveChangesAsync();

        // Revertimos el débito de la venta A.
        cuenta.RevertirDebitoDeVenta(ventaA);
        await contexto.SaveChangesAsync();

        Assert.Equal(70000m, cuenta.SaldoActual);

        var usuario = new Usuario(
            $"test_{identificador[..20]}",
            "Vendedor de prueba",
            "hash-de-prueba-no-utilizado-para-login",
            RolUsuario.Vendedor);

        contexto.Usuarios.Add(usuario);
        await contexto.SaveChangesAsync();

        var pago = new Pago(
            20000m,
            FormaPago.Efectivo,
            usuario.Id);

        contexto.Pagos.Add(pago);
        await contexto.SaveChangesAsync();

        // Act
        var excepcion = Assert.Throws<InvalidOperationException>(
            () => cuenta.RegistrarPagoDeVenta(
                ventaA,
                pago,
                20000m));

        // Assert
        Assert.Contains(
            "débito fue revertido",
            excepcion.Message,
            StringComparison.OrdinalIgnoreCase);

        // La deuda de la venta B debe permanecer intacta.
        Assert.Equal(70000m, cuenta.SaldoActual);

        // No debe existir ningún movimiento asociado al pago.
        Assert.DoesNotContain(
            cuenta.Movimientos,
            m => m.PagoId == pago.Id || m.Pago == pago);

        // La transacción se revierte al finalizar.
    }

    [Fact]
    public async Task ObtenerSaldoPendienteDeVenta_DebeSerCeroTrasRevertirDebito()
    {
        // Arrange
        await using var aplicacion =
            new SistemaVentaWebApplicationFactory();

        using var scope =
            aplicacion.Services.CreateScope();

        var contexto = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        await using var transaccion =
            await contexto.Database.BeginTransactionAsync();

        var identificador = Guid.NewGuid().ToString("N");

        var cliente = new ClienteFisico(
            "Cliente",
            "Prueba",
            Random.Shared.Next(10_000_000, 100_000_000).ToString(),
            "3815551234",
            $"cliente_{identificador}@test.com",
            "Dirección de prueba");

        contexto.Clientes.Add(cliente);
        await contexto.SaveChangesAsync();

        var venta = new Venta(cliente.Id);

        contexto.Ventas.Add(venta);
        await contexto.SaveChangesAsync();

        var cuenta = new CuentaCorriente(
            cliente.Id,
            100000m);

        cuenta.RegistrarDebito(
            50000m,
            "Venta de prueba",
            venta);

        contexto.CuentasCorrientes.Add(cuenta);
        await contexto.SaveChangesAsync();

        // Act: revertir el débito de la venta.
        cuenta.RevertirDebitoDeVenta(venta);

        await contexto.SaveChangesAsync();

        var cuentaId = cuenta.Id;
        var ventaId = venta.Id;

        // Limpiar las entidades que EF Core tiene en memoria.
        contexto.ChangeTracker.Clear();

        // Recuperar la cuenta y sus movimientos desde PostgreSQL.
        var cuentaRecuperada = await contexto.CuentasCorrientes
            .Include(c => c.Movimientos)
            .SingleAsync(c => c.Id == cuentaId);

        // Assert
        Assert.Equal(
            0m,
            cuentaRecuperada.SaldoActual);

        Assert.Equal(
            0m,
            cuentaRecuperada.ObtenerSaldoPendienteDeVenta(ventaId));

        Assert.Single(
            cuentaRecuperada.Movimientos.Where(m => m.EsDebito()));

        Assert.Single(
            cuentaRecuperada.Movimientos.Where(m => m.EsCredito()));

        // Sin Commit: los datos de prueba se revierten.
    }


}