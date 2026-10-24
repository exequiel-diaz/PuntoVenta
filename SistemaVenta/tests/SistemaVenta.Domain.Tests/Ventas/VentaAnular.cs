
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;
using Xunit;

namespace SistemaVenta.Domain.Tests.Ventas
{
    public class VentaAnularTests
    {
        [Fact]
        public void Anular_VentaConfirmada_CambiaEstadoAAnulada()
        {
            // Arrange
            var categoria = new Categoria("Periféricos");

            var producto = new Producto(
                "TEC-001",
                "Teclado",
                15000m,
                10000m,
                2m,
                categoria);

            var venta = new Venta(1);

            venta.AgregarDetalle(producto, 1.5m, 15000m);
            venta.Confirmar();

            // Act
            venta.Anular();

            // Assert
            Assert.Equal(EstadoVenta.Anulada, venta.Estado);
        }

        [Fact]
        public void Anular_VentaPendiente_LanzaExcepcion()
        {
            // Arrange
            var venta = new Venta(1);

            // Act y Assert
            Assert.Throws<InvalidOperationException>(
                () => venta.Anular());

            Assert.Equal(EstadoVenta.Pendiente, venta.Estado);
        }

        [Fact]
        public void Anular_VentaYaAnulada_LanzaExcepcion()
        {
            // Arrange
            var categoria = new Categoria("Periféricos");

            var producto = new Producto(
                "TEC-002",
                "Mouse",
                8000m,
                5000m,
                2m,
                categoria);

            var venta = new Venta(1);

            venta.AgregarDetalle(producto, 1m, 8000m);
            venta.Confirmar();
            venta.Anular();

            // Act y Assert
            Assert.Throws<InvalidOperationException>(
                () => venta.Anular());

            Assert.Equal(EstadoVenta.Anulada, venta.Estado);
        }
    }
}
