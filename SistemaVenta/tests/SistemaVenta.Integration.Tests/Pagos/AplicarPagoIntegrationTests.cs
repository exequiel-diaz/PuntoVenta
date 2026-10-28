
using Microsoft.Extensions.DependencyInjection;
using SistemaVenta.Application.UseCases.Pagos;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;
using SistemaVenta.Infrastructure.Persistence;
using SistemaVenta.Infrastructure.Repositories;
using SistemaVenta.Integration.Tests.Infrastructure;
using Xunit;

namespace SistemaVenta.Integration.Tests.Pagos
{
    public class AplicarPagoIntegrationTests
    {
        [Fact]
        public async Task AplicarPago_Valido_DebePersistirAplicacion()
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
            var dni = Random.Shared.Next(
                10_000_000,
                99_999_999).ToString();

            // 1. Crear cliente.
            var cliente = new ClienteFisico(
                "Cliente",
                "Prueba",
                dni,
                "3815551234",
                $"cliente_{identificador}@test.com",
                "Dirección de prueba");

            contexto.Clientes.Add(cliente);
            await contexto.SaveChangesAsync();

            // 2. Crear venta.
            var venta = new Venta(cliente.Id);

            contexto.Ventas.Add(venta);
            await contexto.SaveChangesAsync();

            // 3. Crear factura de $50.000.
            var factura = new Factura(
                venta.Id,
                TipoFactura.A,
                50000m,
                $"FAC-TEST-{identificador[..12]}");

            contexto.DocumentosComerciales.Add(factura);
            await contexto.SaveChangesAsync();

            // 4. Crear usuario.
            var usuario = new Usuario(
                $"test_{identificador[..20]}",
                "Vendedor de prueba",
                "hash-de-prueba-no-utilizado-para-login",
                RolUsuario.Vendedor);

            contexto.Usuarios.Add(usuario);
            await contexto.SaveChangesAsync();

            // 5. Crear pago de $30.000.
            var pago = new Pago(
                30000m,
                FormaPago.Efectivo,
                usuario.Id);

            var pagoRepository =
                new PagoRepository(contexto);

            await pagoRepository.AgregarAsync(pago);
            await contexto.SaveChangesAsync();

            var pagoId = pago.Id;
            var facturaId = factura.Id;

            // 6. Preparar el caso de uso.

            var documentoRepository =
                new DocumentoComercialRepository(contexto);

            var transaccionPago =
                new TransaccionAplicacionPago(contexto);

            var casoDeUso = new AplicarPago(
                pagoRepository,
                documentoRepository,
                contexto,
                transaccionPago);


            // Act
            await casoDeUso.EjecutarAsync(
                pagoId,
                facturaId,
                20000m);

            // Limpiar las entidades rastreadas para
            // comprobar la persistencia real.
            contexto.ChangeTracker.Clear();

            var pagoRecuperado =
                await pagoRepository.ObtenerPorIdAsync(pagoId);

            // Assert
            Assert.NotNull(pagoRecuperado);

            var aplicacionRecuperada =
                Assert.Single(pagoRecuperado.Aplicaciones);

            Assert.True(aplicacionRecuperada.Id > 0);

            Assert.Equal(
                facturaId,
                aplicacionRecuperada.DocumentoComercialId);

            Assert.Equal(
                20000m,
                aplicacionRecuperada.ImporteAplicado);

            Assert.Equal(
                10000m,
                pagoRecuperado.ImporteDisponible);

            var totalAplicado =
                await documentoRepository.ObtenerTotalAplicadoAsync(
                    facturaId);

            Assert.Equal(20000m, totalAplicado);

            // Sin Commit: los cambios se revierten.
        }
    }
}
