
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaVenta.Application.UseCases.Pagos;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;
using SistemaVenta.Infrastructure.Persistence;
using SistemaVenta.Infrastructure.Repositories;
using SistemaVenta.Integration.Tests.Infrastructure;
using Xunit;

namespace SistemaVenta.Integration.Tests.Pagos;

public class AplicarPagoConcurrenciaTests
{
    [Fact]
    public async Task DosPagosConcurrentes_NoDebenSuperarTotalFactura()
    {
        await using var aplicacion =
            new SistemaVentaWebApplicationFactory();

        using var scopePreparacion =
            aplicacion.Services.CreateScope();

        var contextoPreparacion = scopePreparacion.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var identificador = Guid.NewGuid().ToString("N");

        // DNI de prueba con baja probabilidad de colisión.
        var dni = Random.Shared.Next(
            10_000_000,
            99_999_999).ToString();

        int clienteId = 0;
        int ventaId = 0;
        int facturaId = 0;
        int usuarioId = 0;
        int pagoAId = 0;
        int pagoBId = 0;

        try
        {
            // Preparar datos visibles para ambas conexiones.
            var cliente = new ClienteFisico(
                "Cliente",
                "Concurrencia",
                dni,
                "3815551234",
                $"concurrencia_{identificador}@test.com",
                "Dirección de prueba");

            contextoPreparacion.Clientes.Add(cliente);
            await contextoPreparacion.SaveChangesAsync();
            clienteId = cliente.Id;

            var venta = new Venta(clienteId);
            contextoPreparacion.Ventas.Add(venta);
            await contextoPreparacion.SaveChangesAsync();
            ventaId = venta.Id;

            var factura = new Factura(
                ventaId,
                TipoFactura.A,
                50000m,
                $"CONC-{identificador[..12]}");

            contextoPreparacion.DocumentosComerciales.Add(factura);
            await contextoPreparacion.SaveChangesAsync();
            facturaId = factura.Id;

            var usuario = new Usuario(
                $"conc_{identificador[..20]}",
                "Usuario de concurrencia",
                "hash-de-prueba",
                RolUsuario.Vendedor);

            contextoPreparacion.Usuarios.Add(usuario);
            await contextoPreparacion.SaveChangesAsync();
            usuarioId = usuario.Id;

            var pagoA = new Pago(
                30000m,
                FormaPago.Efectivo,
                usuarioId);

            var pagoB = new Pago(
                30000m,
                FormaPago.Transferencia,
                usuarioId);

            contextoPreparacion.Pagos.AddRange(pagoA, pagoB);
            await contextoPreparacion.SaveChangesAsync();

            pagoAId = pagoA.Id;
            pagoBId = pagoB.Id;

            // Cada operación utiliza su propio DbContext.
            using var scopeA =
                aplicacion.Services.CreateScope();

            using var scopeB =
                aplicacion.Services.CreateScope();

            var contextoA = scopeA.ServiceProvider
                .GetRequiredService<AppDbContext>();

            var contextoB = scopeB.ServiceProvider
                .GetRequiredService<AppDbContext>();

            var casoA = CrearCasoDeUso(contextoA);
            var casoB = CrearCasoDeUso(contextoB);

            using var timeout =
                new CancellationTokenSource(
                    TimeSpan.FromSeconds(20));

            // Ejecutar ambas operaciones concurrentemente.
            var tareaA = CapturarResultadoAsync(
                casoA,
                pagoAId,
                facturaId,
                timeout.Token);

            var tareaB = CapturarResultadoAsync(
                casoB,
                pagoBId,
                facturaId,
                timeout.Token);

            var resultados = await Task.WhenAll(
                tareaA,
                tareaB);

            // Una operación debe completarse.
            Assert.Single(resultados.Where(r => r is null));

            // La otra debe rechazarse por saldo insuficiente.
            Assert.Single(resultados.Where(
                r => r is InvalidOperationException));

            // Verificar los datos realmente persistidos.
            using var scopeVerificacion =
                aplicacion.Services.CreateScope();

            var contextoVerificacion =
                scopeVerificacion.ServiceProvider
                    .GetRequiredService<AppDbContext>();

            var totalAplicado =
                await contextoVerificacion.AplicacionesPago
                    .Where(a =>
                        a.DocumentoComercialId == facturaId)
                    .SumAsync(a => a.ImporteAplicado);

            Assert.Equal(30000m, totalAplicado);

            var cantidadAplicaciones =
                await contextoVerificacion.AplicacionesPago
                    .CountAsync(a =>
                        a.DocumentoComercialId == facturaId);

            Assert.Equal(1, cantidadAplicaciones);

            Assert.Equal(
                20000m,
                50000m - totalAplicado);
        }
        finally
        {
            // Limpiar exclusivamente los datos creados
            // por esta ejecución.
            contextoPreparacion.ChangeTracker.Clear();

            if (facturaId > 0)
            {
                await contextoPreparacion.AplicacionesPago
                    .Where(a =>
                        a.DocumentoComercialId == facturaId)
                    .ExecuteDeleteAsync();
            }

            if (pagoAId > 0 || pagoBId > 0)
            {
                await contextoPreparacion.Pagos
                    .Where(p =>
                        p.Id == pagoAId ||
                        p.Id == pagoBId)
                    .ExecuteDeleteAsync();
            }

            if (facturaId > 0)
            {
                await contextoPreparacion.DocumentosComerciales
                    .Where(d => d.Id == facturaId)
                    .ExecuteDeleteAsync();
            }

            if (ventaId > 0)
            {
                await contextoPreparacion.Ventas
                    .Where(v => v.Id == ventaId)
                    .ExecuteDeleteAsync();
            }

            if (clienteId > 0)
            {
                await contextoPreparacion.Clientes
                    .Where(c => c.Id == clienteId)
                    .ExecuteDeleteAsync();
            }

            if (usuarioId > 0)
            {
                await contextoPreparacion.Usuarios
                    .Where(u => u.Id == usuarioId)
                    .ExecuteDeleteAsync();
            }
        }
    }

    private static AplicarPago CrearCasoDeUso(
        AppDbContext contexto)
    {
        return new AplicarPago(
            new PagoRepository(contexto),
            new DocumentoComercialRepository(contexto),
            contexto,
            new TransaccionAplicacionPago(contexto));
    }

    private static async Task<Exception?> CapturarResultadoAsync(
        AplicarPago casoDeUso,
        int pagoId,
        int facturaId,
        CancellationToken cancellationToken)
    {
        try
        {
            await casoDeUso.EjecutarAsync(
                pagoId,
                facturaId,
                30000m,
                cancellationToken);

            return null;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }
}
