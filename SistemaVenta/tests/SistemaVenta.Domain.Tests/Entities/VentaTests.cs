
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;
using Xunit;

namespace SistemaVenta.Domain.Tests.Entities
{
    public class VentaTests
    {
        private static Producto CrearProducto(
            string codigo = "PROD-001",
            decimal precio = 10000m)
        {
            var categoria = new Categoria(
                "Periféricos",
                "Productos periféricos");

            return new Producto(
                codigoInterno: codigo,
                nombre: "Producto de prueba",
                precioVenta: precio,
                costo: 5000m,
                stockMinimo: 2m,
                categoria: categoria);
        }

        [Fact]
        public void CrearVenta_ConClienteValido_IniciaPendiente()
        {
            var venta = new Venta(clienteId: 1);

            Assert.Equal(1, venta.ClienteId);
            Assert.Equal(EstadoVenta.Pendiente, venta.Estado);
            Assert.Empty(venta.Detalles);
            Assert.Equal(0m, venta.Total);
        }

        [Fact]
        public void CrearVenta_ConClienteInvalido_LanzaExcepcion()
        {
            Assert.Throws<ArgumentException>(() =>
                new Venta(clienteId: 0));
        }

        [Fact]
        public void AgregarDetalle_ConDatosValidos_CalculaTotal()
        {
            var venta = new Venta(1);
            var producto = CrearProducto();

            venta.AgregarDetalle(
                producto,
                cantidad: 2m,
                precioUnitario: 10000m);

            Assert.Single(venta.Detalles);
            Assert.Equal(20000m, venta.Total);
        }

        [Fact]
        public void AgregarVariosDetalles_SumaSubtotales()
        {
            var venta = new Venta(1);

            var producto1 = CrearProducto("PROD-001", 10000m);
            var producto2 = CrearProducto("PROD-002", 15000m);

            venta.AgregarDetalle(producto1, 2m, 10000m);
            venta.AgregarDetalle(producto2, 1m, 15000m);

            Assert.Equal(2, venta.Detalles.Count);
            Assert.Equal(35000m, venta.Total);
        }

        [Fact]
        public void AgregarDetalle_ProductoInactivo_LanzaExcepcion()
        {
            var venta = new Venta(1);
            var producto = CrearProducto();

            producto.Desactivar();

            Assert.Throws<InvalidOperationException>(() =>
                venta.AgregarDetalle(producto, 1m, 10000m));
        }

        [Fact]
        public void AgregarDetalle_ProductoRepetido_LanzaExcepcion()
        {
            var venta = new Venta(1);
            var producto = CrearProducto();

            venta.AgregarDetalle(producto, 1m, 10000m);

            Assert.Throws<InvalidOperationException>(() =>
                venta.AgregarDetalle(producto, 2m, 10000m));
        }

        [Fact]
        public void ConfirmarVenta_SinDetalles_LanzaExcepcion()
        {
            var venta = new Venta(1);

            Assert.Throws<InvalidOperationException>(() =>
                venta.Confirmar());
        }

        [Fact]
        public void ConfirmarVenta_ConDetalles_CambiaEstado()
        {
            var venta = new Venta(1);
            var producto = CrearProducto();

            venta.AgregarDetalle(producto, 1m, 10000m);

            venta.Confirmar();

            Assert.Equal(EstadoVenta.Confirmada, venta.Estado);
        }

        [Fact]
        public void ConfirmarVenta_DosVeces_LanzaExcepcion()
        {
            var venta = new Venta(1);
            var producto = CrearProducto();

            venta.AgregarDetalle(producto, 1m, 10000m);
            venta.Confirmar();

            Assert.Throws<InvalidOperationException>(() =>
                venta.Confirmar());
        }

        [Fact]
        public void AgregarDetalle_VentaConfirmada_LanzaExcepcion()
        {
            var venta = new Venta(1);
            var producto1 = CrearProducto("PROD-001");
            var producto2 = CrearProducto("PROD-002");

            venta.AgregarDetalle(producto1, 1m, 10000m);
            venta.Confirmar();

            Assert.Throws<InvalidOperationException>(() =>
                venta.AgregarDetalle(producto2, 1m, 10000m));
        }
    }
}
