using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaVenta.Application.UseCases.CuentasCorrientes;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;
using SistemaVenta.Infrastructure.Persistence;
using SistemaVenta.Integration.Tests.Infrastructure;
using Xunit;

namespace SistemaVenta.Integration.Tests.CuentasCorrientes;

public class RegistrarCobroVentaCuentaCorrienteIntegrationTests
{
    [Fact]
    public async Task RegistrarCobroVenta_DebeRegistrarPagoYReducirDeuda()
    {
        // Arrange
        await using var aplicacion =
            new SistemaVentaWebApplicationFactory();

        using var scope =
            aplicacion.Services.CreateScope();

        var contexto = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var registrarCobro = scope.ServiceProvider
            .GetRequiredService<RegistrarCobroVentaCuentaCorriente>();

        // Evitamos dejar datos de prueba en la base.
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

        // 2. Crear categoría y producto.
        var categoria = new Categoria(
            $"Categoría {identificador}");

        contexto.Categorias.Add(categoria);

        var producto = new Producto(
            $"RAM-{identificador[..12]}",
            "Memoria RAM de prueba",
            50000m,
            30000m,
            2,
            categoria);

        contexto.Productos.Add(producto);
        await contexto.SaveChangesAsync();

        // 3. Crear y confirmar venta.
        var venta = new Venta(cliente.Id);

        venta.AgregarDetalle(
            producto,
            1,
            50000m);

        venta.Confirmar();

        contexto.Ventas.Add(venta);
        await contexto.SaveChangesAsync();

        // 4. Registrar deuda en cuenta corriente.
        var cuenta = new CuentaCorriente(
            cliente.Id,
            100000m);

        cuenta.RegistrarDebito(
            50000m,
            "Venta de memoria RAM",
            venta);

        contexto.CuentasCorrientes.Add(cuenta);
        await contexto.SaveChangesAsync();

        // 5. Crear usuario vendedor.
        var usuario = new Usuario(
            $"test_{identificador[..20]}",
            "Vendedor de prueba",
            "hash-de-prueba",
            RolUsuario.Vendedor);

        contexto.Usuarios.Add(usuario);
        await contexto.SaveChangesAsync();

        // Act: cobrar $20.000.
        var pagoId = await registrarCobro.EjecutarAsync(
            venta.Id,
            20000m,
            FormaPago.Efectivo,
            usuario.Id);

        // Consultar nuevamente los datos de PostgreSQL.
        contexto.ChangeTracker.Clear();

        var pagoRecuperado = await contexto.Pagos
            .SingleAsync(p => p.Id == pagoId);

        var cuentaRecuperada = await contexto.CuentasCorrientes
            .Include(c => c.Movimientos)
            .SingleAsync(c => c.Id == cuenta.Id);

        // Assert
        Assert.Equal(20000m, pagoRecuperado.Importe);

        Assert.Equal(
            30000m,
            cuentaRecuperada.SaldoActual);

        Assert.Equal(
            30000m,
            cuentaRecuperada.ObtenerSaldoPendienteDeVenta(
                venta.Id));

        var movimientoPago = Assert.Single(
            cuentaRecuperada.Movimientos.Where(m =>
                m.EsCredito() &&
                m.PagoId == pagoId));

        Assert.Equal(20000m, movimientoPago.Importe);
        Assert.Equal(venta.Id, movimientoPago.VentaId);

        // Sin Commit: se revierten los datos de prueba.
    }


    [Fact]
    public async Task RegistrarCobroVenta_NoDebeGuardarPagoSiSuperaDeuda()
    {
        // ARRANGE
        await using var aplicacion =
            new SistemaVentaWebApplicationFactory();

        using var scope =
            aplicacion.Services.CreateScope();

        var contexto = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var registrarCobro = scope.ServiceProvider
            .GetRequiredService<RegistrarCobroVentaCuentaCorriente>();

        // Los datos creados por la prueba se revierten al finalizar.
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

        // 2. Crear categoría y producto.
        var categoria = new Categoria(
            $"Categoría {identificador}");

        contexto.Categorias.Add(categoria);

        var producto = new Producto(
            $"RAM-{identificador[..12]}",
            "Memoria RAM de prueba",
            50000m,
            30000m,
            2,
            categoria);

        contexto.Productos.Add(producto);
        await contexto.SaveChangesAsync();

        // 3. Crear y confirmar venta de $50.000.
        var venta = new Venta(cliente.Id);

        venta.AgregarDetalle(
            producto,
            1,
            50000m);

        venta.Confirmar();

        contexto.Ventas.Add(venta);
        await contexto.SaveChangesAsync();

        // 4. Registrar deuda en cuenta corriente.
        var cuenta = new CuentaCorriente(
            cliente.Id,
            100000m);

        cuenta.RegistrarDebito(
            50000m,
            "Venta de prueba",
            venta);

        contexto.CuentasCorrientes.Add(cuenta);
        await contexto.SaveChangesAsync();

        // 5. Crear vendedor.
        var usuario = new Usuario(
            $"test_{identificador[..20]}",
            "Vendedor de prueba",
            "hash-de-prueba",
            RolUsuario.Vendedor);

        contexto.Usuarios.Add(usuario);
        await contexto.SaveChangesAsync();

        // 6. Consultar los pagos asociados a esta venta.
        var pagosAntes = await contexto.Pagos
            .CountAsync(p => p.AplicacionesVentas
                .Any(a => a.VentaId == venta.Id));

        // ACT
        // Intentamos cobrar $60.000 cuando la deuda es $50.000.
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => registrarCobro.EjecutarAsync(
                venta.Id,
                60000m,
                FormaPago.Efectivo,
                usuario.Id));

        // Volver a consultar los datos desde PostgreSQL.
        contexto.ChangeTracker.Clear();

        // 7. Consultar nuevamente los pagos de ESTA venta.
        var pagosDespues = await contexto.Pagos
            .CountAsync(p => p.AplicacionesVentas
                .Any(a => a.VentaId == venta.Id));

        // 8. Recuperar la cuenta corriente.
        var cuentaRecuperada = await contexto.CuentasCorrientes
            .Include(c => c.Movimientos)
            .SingleAsync(c => c.Id == cuenta.Id);

        // ASSERT

        // No debe existir ningún pago aplicado a esta venta.
        Assert.Equal(pagosAntes, pagosDespues);
        Assert.Equal(0, pagosDespues);

        // La deuda debe continuar siendo $50.000.
        Assert.Equal(
            50000m,
            cuentaRecuperada.SaldoActual);

        // El saldo pendiente de esta venta no debe cambiar.
        Assert.Equal(
            50000m,
            cuentaRecuperada.ObtenerSaldoPendienteDeVenta(
                venta.Id));

        // No debe generarse ningún movimiento de crédito.
        Assert.DoesNotContain(
            cuentaRecuperada.Movimientos,
            m => m.EsCredito());

        // Sin Commit: se revierten los datos de prueba.
    }

}