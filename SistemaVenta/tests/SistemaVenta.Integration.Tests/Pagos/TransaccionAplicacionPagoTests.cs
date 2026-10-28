
using Microsoft.Extensions.DependencyInjection;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;
using SistemaVenta.Infrastructure.Persistence;
using SistemaVenta.Integration.Tests.Infrastructure;
using Xunit;

namespace SistemaVenta.Integration.Tests.Pagos
{
    public class TransaccionAplicacionPagoTests
    {
        [Fact]
        public async Task EjecutarAsync_FacturaExistente_DebeEjecutarOperacion()
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
            var dni = Random.Shared.Next(10_000_000, 100_000_000).ToString();


            var cliente = new ClienteFisico(
                "Cliente",
                "Prueba",
                dni,
                "3815551234",
                $"cliente_{identificador}@test.com",
                "Dirección de prueba");


            contexto.Clientes.Add(cliente);
            await contexto.SaveChangesAsync();

            var venta = new Venta(cliente.Id);

            contexto.Ventas.Add(venta);
            await contexto.SaveChangesAsync();

            var factura = new Factura(
                venta.Id,
                TipoFactura.A,
                50000m,
                $"FAC-LOCK-{identificador[..12]}");

            contexto.DocumentosComerciales.Add(factura);
            await contexto.SaveChangesAsync();

            var transaccionPago =
                new TransaccionAplicacionPago(contexto);

            var operacionEjecutada = false;

            // Act
            await transaccionPago.EjecutarAsync(
                factura.Id,
                () =>
                {
                    operacionEjecutada = true;
                    return Task.CompletedTask;
                });

            // Assert
            Assert.True(operacionEjecutada);
        }
    }
}
