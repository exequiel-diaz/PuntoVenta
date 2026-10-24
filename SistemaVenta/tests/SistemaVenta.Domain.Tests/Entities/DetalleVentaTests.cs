
using SistemaVenta.Domain.Entities;
using Xunit;

namespace SistemaVenta.Domain.Tests.Entities
{
    public class DetalleVentaTests
    {
        private static Producto CrearProducto()
        {
            var categoria = new Categoria(
                "Periféricos",
                "Productos periféricos");

            return new Producto(
                codigoInterno: "TEC-001",
                nombre: "Teclado",
                precioVenta: 25000m,
                costo: 15000m,
                stockMinimo: 2m,
                categoria: categoria);
        }

        [Fact]
        public void CrearDetalle_ConDatosValidos_CalculaSubtotal()
        {
            // Arrange
            var producto = CrearProducto();

            // Act
            var detalle = new DetalleVenta(
                producto,
                cantidad: 2m,
                precioUnitario: 25000m);

            // Assert
            Assert.Equal(2m, detalle.Cantidad);
            Assert.Equal(25000m, detalle.PrecioUnitario);
            Assert.Equal(50000m, detalle.Subtotal);
            Assert.Same(producto, detalle.Producto);
        }

        [Fact]
        public void CrearDetalle_ConCantidadCero_LanzaExcepcion()
        {
            var producto = CrearProducto();

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new DetalleVenta(producto, 0m, 25000m));
        }

        [Fact]
        public void CrearDetalle_ConCantidadNegativa_LanzaExcepcion()
        {
            var producto = CrearProducto();

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new DetalleVenta(producto, -2m, 25000m));
        }

        [Fact]
        public void CrearDetalle_ConPrecioNegativo_LanzaExcepcion()
        {
            var producto = CrearProducto();

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new DetalleVenta(producto, 2m, -100m));
        }

        [Fact]
        public void CrearDetalle_SinProducto_LanzaExcepcion()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new DetalleVenta(null!, 2m, 25000m));
        }

        [Fact]
        public void CrearDetalle_ConPrecioCero_EsValido()
        {
            var producto = CrearProducto();

            var detalle = new DetalleVenta(
                producto,
                cantidad: 1m,
                precioUnitario: 0m);

            Assert.Equal(0m, detalle.Subtotal);
        }
    }
}
