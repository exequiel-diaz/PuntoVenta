using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaVenta.Application.UseCases.CuentasCorrientes;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;
using SistemaVenta.Infrastructure.Persistence;
using SistemaVenta.Integration.Tests.Infrastructure;
using Xunit;

namespace SistemaVenta.Integration.Tests.CuentasCorrientes;

public class AplicarPagoVentaCuentaCorrienteIntegrationTests
{
    [Fact]
    public async Task EjecutarAsync_DebeAplicarPagoYReducirDeuda()
    {
        // Arrange
        await using var aplicacion =
            new SistemaVentaWebApplicationFactory();

        using var scope = aplicacion.Services.CreateScope();

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

        // Crear una categoría.
        var categoria = new Categoria(
            $"Computación {identificador[..8]}");

        contexto.Categorias.Add(categoria);
        await contexto.SaveChangesAsync();

        // Crear un producto.
        var producto = new Producto(
            $"PROD-{identificador[..12]}",
            "Teclado mecánico",
            50000m,
            30000m,
            5m,
            categoria);

        contexto.Productos.Add(producto);
        await contexto.SaveChangesAsync();

        // Crear y confirmar una venta válida.
        var venta = new Venta(cliente.Id);

        venta.AgregarDetalle(
            producto,
            1m,
            50000m);

        venta.Confirmar();

        contexto.Ventas.Add(venta);
        await contexto.SaveChangesAsync();

        Assert.Equal(EstadoVenta.Confirmada, venta.Estado);
        Assert.Equal(50000m, venta.Total);

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

        var pago = new Pago(
            20000m,
            FormaPago.Efectivo,
            usuario.Id);

        contexto.Pagos.Add(pago);
        await contexto.SaveChangesAsync();

        // Act: ejecutar el caso de uso.
        var casoDeUso = scope.ServiceProvider
            .GetRequiredService<AplicarPagoVentaCuentaCorriente>();

        await casoDeUso.EjecutarAsync(
            venta.Id,
            pago.Id,
            20000m);

        // Intentar aplicar nuevamente el mismo pago,
        // aunque ya no tiene importe disponible.
        var excepcion = await Assert.ThrowsAsync<InvalidOperationException>(
            () => casoDeUso.EjecutarAsync(
                venta.Id,
                pago.Id,
                5000m));

        Assert.Contains(
            "disponible",
            excepcion.Message,
            StringComparison.OrdinalIgnoreCase);

        // Crear otro pago con dinero suficiente.
        var segundoPago = new Pago(
            40000m,
            FormaPago.Efectivo,
            usuario.Id);

        contexto.Pagos.Add(segundoPago);
        await contexto.SaveChangesAsync();

        // La venta debe $30.000, pero intentamos aplicar $35.000.
        var excepcionSaldo = await Assert.ThrowsAsync<InvalidOperationException>(
            () => casoDeUso.EjecutarAsync(
                venta.Id,
                segundoPago.Id,
                35000m));

        Assert.Contains(
            "saldo pendiente",
            excepcionSaldo.Message,
            StringComparison.OrdinalIgnoreCase);

        // Guardar identificadores antes de limpiar el contexto.
        var cuentaId = cuenta.Id;
        var ventaId = venta.Id;
        var pagoId = pago.Id;

        // Eliminar las entidades cargadas en memoria.
        contexto.ChangeTracker.Clear();

        // Recuperar el pago y sus aplicaciones desde PostgreSQL.
        var pagoRecuperado = await contexto.Pagos
            .Include(p => p.AplicacionesVentas)
            .SingleAsync(p => p.Id == pagoId);

        // Recuperar la cuenta y sus movimientos.
        var cuentaRecuperada = await contexto.CuentasCorrientes
            .Include(c => c.Movimientos)
            .SingleAsync(c => c.Id == cuentaId);

        // Assert: verificar la aplicación del pago.
        Assert.Equal(
            0m,
            pagoRecuperado.ImporteDisponible);

        var aplicacionPago = Assert.Single(
            pagoRecuperado.AplicacionesVentas);

        Assert.Equal(ventaId, aplicacionPago.VentaId);
        Assert.Equal(20000m, aplicacionPago.ImporteAplicado);

        // Assert: verificar la deuda.
        Assert.Equal(
            30000m,
            cuentaRecuperada.SaldoActual);

        Assert.Equal(
            30000m,
            cuentaRecuperada.ObtenerSaldoPendienteDeVenta(ventaId));

        // Assert: verificar el movimiento de crédito.
        var movimientoPago = Assert.Single(
            cuentaRecuperada.Movimientos.Where(m =>
                m.EsCredito() &&
                m.PagoId == pagoId));

        Assert.Equal(20000m, movimientoPago.Importe);
        Assert.Equal(ventaId, movimientoPago.VentaId);

        // Recuperar el segundo pago después del intento rechazado.
        contexto.ChangeTracker.Clear();

        var segundoPagoRecuperado = await contexto.Pagos
            .Include(p => p.AplicacionesVentas)
            .SingleAsync(p => p.Id == segundoPago.Id);

        // El segundo pago debe conservar todo su importe disponible.
        Assert.Equal(
            40000m,
            segundoPagoRecuperado.ImporteDisponible);

        // No debe tener aplicaciones a ventas.
        Assert.Empty(
            segundoPagoRecuperado.AplicacionesVentas);
    }

    [Fact]
    public async Task TransaccionPagoVenta_DebeRevertirCambiosSiOcurreUnError()
    {
        await using var aplicacion =
            new SistemaVentaWebApplicationFactory();

        using var scope = aplicacion.Services.CreateScope();

        var contexto = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var transaccionPago = scope.ServiceProvider
            .GetRequiredService<
                SistemaVenta.Application.Interfaces.ITransaccionPagoVenta>();

        var identificador = Guid.NewGuid().ToString("N");

        // Preparar un cliente.
        var cliente = new ClienteFisico(
            "Cliente",
            "Prueba",
            Random.Shared.Next(10_000_000, 100_000_000).ToString(),
            "3815551234",
            $"cliente_{identificador}@test.com",
            "Dirección de prueba");

        contexto.Clientes.Add(cliente);
        await contexto.SaveChangesAsync();

        // Crear una venta para disponer de un identificador válido.
        var venta = new Venta(cliente.Id);

        contexto.Ventas.Add(venta);
        await contexto.SaveChangesAsync();

        // Preparar un usuario.
        var usuario = new Usuario(
            $"test_{identificador[..20]}",
            "Vendedor de prueba",
            "hash-de-prueba-no-utilizado-para-login",
            RolUsuario.Vendedor);

        contexto.Usuarios.Add(usuario);
        await contexto.SaveChangesAsync();

        // Registrar un pago sin aplicaciones.
        var pago = new Pago(
            20000m,
            FormaPago.Efectivo,
            usuario.Id);

        contexto.Pagos.Add(pago);
        await contexto.SaveChangesAsync();

        var ventaId = venta.Id;
        var pagoId = pago.Id;

        // Comprobar el estado inicial.
        Assert.Equal(EstadoPago.Registrado, pago.Estado);

        // Ejecutar una operación que guarda y después falla.
        var excepcion = await Assert.ThrowsAsync<InvalidOperationException>(
            () => transaccionPago.EjecutarAsync(
                ventaId,
                pagoId,
                async () =>
                {
                    var pagoActual = await contexto.Pagos
                        .SingleAsync(p => p.Id == pagoId);

                    // Esta operación es válida.
                    pagoActual.Anular();

                    // El cambio se envía a PostgreSQL,
                    // pero todavía no se confirma la transacción.
                    await contexto.SaveChangesAsync();

                    // Simulamos un error posterior al guardado.
                    throw new InvalidOperationException(
                        "Error simulado después de guardar.");
                }));

        Assert.Equal(
            "Error simulado después de guardar.",
            excepcion.Message);

        // Descartar las entidades que EF Core mantiene en memoria.
        contexto.ChangeTracker.Clear();

        // Consultar nuevamente el estado real de PostgreSQL.
        var pagoRecuperado = await contexto.Pagos
            .AsNoTracking()
            .SingleAsync(p => p.Id == pagoId);

        // La anulación debe haberse revertido.
        Assert.Equal(
            EstadoPago.Registrado,
            pagoRecuperado.Estado);

        Assert.Equal(
            20000m,
            pagoRecuperado.ImporteDisponible);
    }

    [Fact]
    public async Task TransaccionPagoVenta_DebeBloquearElMismoPago()
    {
        await using var aplicacion =
            new SistemaVentaWebApplicationFactory();

        using var scope1 = aplicacion.Services.CreateScope();
        using var scope2 = aplicacion.Services.CreateScope();

        var contexto1 = scope1.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var contexto2 = scope2.ServiceProvider
            .GetRequiredService<AppDbContext>();

        Assert.NotSame(contexto1, contexto2);
        // Preparar datos compartidos en PostgreSQL.
        var identificador = Guid.NewGuid().ToString("N");

        var cliente = new ClienteFisico(
            "Cliente",
            "Concurrencia",
            Random.Shared.Next(10_000_000, 100_000_000).ToString(),
            "3815551234",
            $"concurrencia_{identificador}@test.com",
            "Dirección de prueba");

        contexto1.Clientes.Add(cliente);
        await contexto1.SaveChangesAsync();

        var venta = new Venta(cliente.Id);

        contexto1.Ventas.Add(venta);
        await contexto1.SaveChangesAsync();

        var usuario = new Usuario(
            $"test_{identificador[..20]}",
            "Vendedor de prueba",
            "hash-de-prueba-no-utilizado-para-login",
            RolUsuario.Vendedor);

        contexto1.Usuarios.Add(usuario);
        await contexto1.SaveChangesAsync();

        var pago = new Pago(
            20000m,
            FormaPago.Efectivo,
            usuario.Id);

        contexto1.Pagos.Add(pago);
        await contexto1.SaveChangesAsync();

        // Ambos contextos utilizarán estos mismos identificadores.
        var ventaId = venta.Id;
        var pagoId = pago.Id;

        // Verificar que el segundo contexto puede consultar el pago.
        var pagoDesdeSegundoContexto = await contexto2.Pagos
            .AsNoTracking()
            .SingleAsync(p => p.Id == pagoId);

        Assert.Equal(20000m, pagoDesdeSegundoContexto.Importe);
        Assert.Equal(EstadoPago.Registrado, pagoDesdeSegundoContexto.Estado);

        // Obtener los servicios transaccionales de cada scope.
        var transaccion1 = scope1.ServiceProvider
            .GetRequiredService<
                SistemaVenta.Application.Interfaces.ITransaccionPagoVenta>();

        var transaccion2 = scope2.ServiceProvider
            .GetRequiredService<
                SistemaVenta.Application.Interfaces.ITransaccionPagoVenta>();

        // Señal para saber cuándo la primera transacción
        // ya adquirió los bloqueos.
        var bloqueoAdquirido = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        // Señal para liberar la primera transacción.
        var liberarBloqueo = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        // Primera operación: adquiere los bloqueos y espera.
        var tarea1 = transaccion1.EjecutarAsync(
            ventaId,
            pagoId,
            async () =>
            {
                bloqueoAdquirido.SetResult(true);
                await liberarBloqueo.Task;
            });

        try
        {
            // Esperar a que la primera operación tenga el bloqueo.
            await bloqueoAdquirido.Task.WaitAsync(
                TimeSpan.FromSeconds(10));

            // Segunda operación: intenta bloquear el mismo pago.
            var tarea2 = transaccion2.EjecutarAsync(
                ventaId,
                pagoId,
                () => Task.CompletedTask);

            // Darle un breve intervalo para intentar adquirirlo.
            await Task.Delay(300);

            // No debería haber terminado mientras A mantiene el bloqueo.
            Assert.False(
                tarea2.IsCompleted,
                "La segunda operación terminó mientras el pago seguía bloqueado.");

            // Liberar la primera transacción.
            liberarBloqueo.SetResult(true);

            // Ambas operaciones deben terminar correctamente.
            await tarea1.WaitAsync(TimeSpan.FromSeconds(10));
            await tarea2.WaitAsync(TimeSpan.FromSeconds(10));
        }
        finally
        {
            // Evitar que la primera operación quede esperando
            // si alguna comprobación falla.
            liberarBloqueo.TrySetResult(true);
        }

    }

    [Fact]
    public async Task EjecutarAsync_DosCobrosConcurrentes_NoDebenSuperarImporteDisponible()
    {
        await using var aplicacion =
            new SistemaVentaWebApplicationFactory();

        using var scopePreparacion =
            aplicacion.Services.CreateScope();

        var contexto = scopePreparacion.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var identificador = Guid.NewGuid().ToString("N");

        var cliente = new ClienteFisico(
            "Cliente",
            "Concurrencia",
            Random.Shared.Next(10_000_000, 100_000_000).ToString(),
            "3815551234",
            $"concurrencia_{identificador}@test.com",
            "Dirección de prueba");

        contexto.Clientes.Add(cliente);
        await contexto.SaveChangesAsync();

        var categoria = new Categoria(
            $"Computación {identificador[..8]}");

        contexto.Categorias.Add(categoria);
        await contexto.SaveChangesAsync();

        var producto = new Producto(
            $"PROD-{identificador[..12]}",
            "Teclado mecánico",
            50000m,
            30000m,
            5m,
            categoria);

        contexto.Productos.Add(producto);
        await contexto.SaveChangesAsync();

        var venta = new Venta(cliente.Id);

        venta.AgregarDetalle(
            producto,
            1m,
            50000m);

        venta.Confirmar();

        contexto.Ventas.Add(venta);
        await contexto.SaveChangesAsync();

        var cuenta = new CuentaCorriente(
            cliente.Id,
            100000m);

        cuenta.RegistrarDebito(
            50000m,
            "Venta de prueba de concurrencia",
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

        var pago = new Pago(
            20000m,
            FormaPago.Efectivo,
            usuario.Id);

        contexto.Pagos.Add(pago);
        await contexto.SaveChangesAsync();

        // Identificadores que utilizarán las operaciones concurrentes.
        var ventaId = venta.Id;
        var pagoId = pago.Id;

        // Verificar las condiciones iniciales.
        Assert.Equal(EstadoVenta.Confirmada, venta.Estado);
        Assert.Equal(50000m, venta.Total);
        Assert.Equal(50000m, cuenta.SaldoActual);
        Assert.Equal(20000m, pago.ImporteDisponible);
        Assert.True(ventaId > 0);
        Assert.True(pagoId > 0);

        // Crear dos scopes independientes.
        using var scope1 = aplicacion.Services.CreateScope();
        using var scope2 = aplicacion.Services.CreateScope();

        // Cada scope obtiene su propia instancia del caso de uso.
        var casoDeUso1 = scope1.ServiceProvider
            .GetRequiredService<AplicarPagoVentaCuentaCorriente>();

        var casoDeUso2 = scope2.ServiceProvider
            .GetRequiredService<AplicarPagoVentaCuentaCorriente>();

        // Comprobar que utilizan contextos diferentes.
        var contexto1 = scope1.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var contexto2 = scope2.ServiceProvider
            .GetRequiredService<AppDbContext>();

        Assert.NotSame(contexto1, contexto2);

        // Lanzar ambos cobros sin esperar individualmente.
        var tarea1 = casoDeUso1.EjecutarAsync(
            ventaId,
            pagoId,
            15000m);

        var tarea2 = casoDeUso2.EjecutarAsync(
            ventaId,
            pagoId,
            15000m);

        // Esperar a que ambas operaciones finalicen.
        var resultados = await Task.WhenAll(
            CapturarResultadoAsync(tarea1),
            CapturarResultadoAsync(tarea2));

        // Debe existir exactamente una operación exitosa.
        Assert.Single(resultados.Where(r => r is null));

        // La otra operación debe ser rechazada.
        var error = Assert.Single(
            resultados.OfType<InvalidOperationException>());

        Assert.Contains(
            "disponible",
            error.Message,
            StringComparison.OrdinalIgnoreCase);

        // Consultar PostgreSQL desde un tercer contexto independiente.
        using var scopeVerificacion = aplicacion.Services.CreateScope();

        var contextoVerificacion = scopeVerificacion.ServiceProvider
            .GetRequiredService<AppDbContext>();

        // Recuperar el pago y sus aplicaciones.
        var pagoRecuperado = await contextoVerificacion.Pagos
            .AsNoTracking()
            .Include(p => p.Aplicaciones)
            .Include(p => p.AplicacionesVentas)
            .SingleAsync(p => p.Id == pagoId);

        // Debe existir una sola aplicación a la venta.
        var aplicacionRegistrada = Assert.Single(
            pagoRecuperado.AplicacionesVentas);

        Assert.Equal(
            ventaId,
            aplicacionRegistrada.VentaId);

        Assert.Equal(
            15000m,
            aplicacionRegistrada.ImporteAplicado);

        // El pago debe conservar $5.000 disponibles.
        Assert.Equal(
            5000m,
            pagoRecuperado.ImporteDisponible);

        // Recuperar la cuenta corriente con sus movimientos.
        var cuentaRecuperada = await contextoVerificacion.CuentasCorrientes
            .AsNoTracking()
            .Include(c => c.Movimientos)
            .SingleAsync(c => c.ClienteId == cliente.Id);

        // La deuda debe haberse reducido a $35.000.
        Assert.Equal(
            35000m,
            cuentaRecuperada.SaldoActual);

        Assert.Equal(
            35000m,
            cuentaRecuperada.ObtenerSaldoPendienteDeVenta(ventaId));

        // Debe existir un solo movimiento asociado al pago.
        var movimientosDelPago = cuentaRecuperada.Movimientos
            .Where(m => m.PagoId == pagoId)
            .ToList();

        var movimientoRegistrado = Assert.Single(movimientosDelPago);

        Assert.Equal(
            15000m,
            movimientoRegistrado.Importe);

        Assert.Equal(
            ventaId,
            movimientoRegistrado.VentaId);


    }

    private static async Task<Exception?> CapturarResultadoAsync(Task tarea)
    {
        try
        {
            await tarea;
            return null;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

}