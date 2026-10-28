
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaVenta.Infrastructure.Persistence;
using SistemaVenta.Integration.Tests.Infrastructure;
using Xunit;

namespace SistemaVenta.Integration.Tests.Pagos
{
    public class TransaccionAplicacionPagoConcurrenciaTests
    {
        [Fact]
        public async Task BloquearMismaFactura_DebeEsperarLiberacion()
        {
            // Arrange
            await using var aplicacion =
                new SistemaVentaWebApplicationFactory();

            using var scope1 =
                aplicacion.Services.CreateScope();

            using var scope2 =
                aplicacion.Services.CreateScope();

            var contexto1 = scope1.ServiceProvider
                .GetRequiredService<AppDbContext>();

            var contexto2 = scope2.ServiceProvider
                .GetRequiredService<AppDbContext>();

            // Buscar una factura existente en la base de pruebas.
            var facturaId = await contexto1.DocumentosComerciales
                .OfType<SistemaVenta.Domain.Entities.Factura>()
                .AsNoTracking()
                .Select(f => f.Id)
                .FirstOrDefaultAsync();

            Assert.True(
                facturaId > 0,
                "Se necesita una factura en sistema_venta_tests.");

            var transaccionPago1 =
                new TransaccionAplicacionPago(contexto1);

            var transaccionPago2 =
                new TransaccionAplicacionPago(contexto2);

            var primerBloqueoAdquirido =
                new TaskCompletionSource<bool>(
                    TaskCreationOptions.RunContinuationsAsynchronously);

            var liberarPrimerBloqueo =
                new TaskCompletionSource<bool>(
                    TaskCreationOptions.RunContinuationsAsynchronously);

            var segundoBloqueoAdquirido = false;

            using var timeout =
                new CancellationTokenSource(
                    TimeSpan.FromSeconds(20));

            // Act
            var tarea1 = transaccionPago1.EjecutarAsync(
                facturaId,
                async () =>
                {
                    primerBloqueoAdquirido.SetResult(true);

                    await liberarPrimerBloqueo.Task
                        .WaitAsync(timeout.Token);
                },
                timeout.Token);

            try
            {
                // Esperar a que la primera transacción
                // tenga bloqueada la factura.
                await primerBloqueoAdquirido.Task
                    .WaitAsync(timeout.Token);

                var tarea2 = transaccionPago2.EjecutarAsync(
                    facturaId,
                    () =>
                    {
                        segundoBloqueoAdquirido = true;
                        return Task.CompletedTask;
                    },
                    timeout.Token);

                // Dar tiempo a la segunda operación
                // para intentar adquirir el bloqueo.
                await Task.Delay(500, timeout.Token);

                // Assert: todavía no debe haber entrado.
                Assert.False(segundoBloqueoAdquirido);

                // Liberar el primer bloqueo.
                liberarPrimerBloqueo.SetResult(true);

                await Task.WhenAll(tarea1, tarea2);

                // Ahora la segunda operación debe ejecutarse.
                Assert.True(segundoBloqueoAdquirido);
            }
            finally
            {
                // Evitar dejar bloqueada la primera operación
                // si alguna comprobación falla.
                liberarPrimerBloqueo.TrySetResult(true);

                await tarea1;
            }
        }
    }
}
