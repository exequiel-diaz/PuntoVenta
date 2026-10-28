
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Application.Tests.Fakes;
using SistemaVenta.Application.UseCases.Ventas;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;
using System.Reflection;
using Xunit;

namespace SistemaVenta.Application.Tests.Ventas
{
    public class AnularVentaTests
    {
        private readonly FakeVentaRepository _ventas = new();
        private readonly FakeProductoRepository _productos = new();
        private readonly FakeMovimientoStockRepository _movimientos = new();
        private readonly FakeUnitOfWork _unitOfWork = new();
        private readonly FakeDocumentoComercialRepository _documentos = new();

        private AnularVenta CrearCasoDeUso()
        {
            return new AnularVenta(
                _ventas,
                _productos,
                _movimientos,
                _unitOfWork,
                _documentos,
                new FakeTransaccionVenta());
        }


        private static void AsignarId(object entidad, int id)
        {
            var propiedad = entidad.GetType().GetProperty(
                "Id",
                BindingFlags.Instance | BindingFlags.Public);

            propiedad!.SetValue(entidad, id);
        }

        [Fact]
        public async Task AnularVenta_Confirmada_RestauraStock()
        {
            // Arrange
            var categoria = new Categoria(
                "Periféricos",
                "Productos periféricos");

            var producto = new Producto(
                codigoInterno: "TEC-001",
                nombre: "Teclado",
                precioVenta: 15000m,
                costo: 10000m,
                stockMinimo: 2m,
                categoria: categoria);

            AsignarId(producto, 1);

            producto.AumentarStock(10m);

            var venta = new Venta(1);

            venta.AgregarDetalle(
                producto,
                cantidad: 1.5m,
                precioUnitario: 15000m);

            venta.Confirmar();

            AsignarId(venta, 1);

            // Simulamos el descuento realizado al vender.
            producto.DisminuirStock(1.5m);

            _productos.ConfigurarProducto(1, producto);

            // Registramos la venta existente.
            await _ventas.AgregarAsync(venta);

            // Act
            await CrearCasoDeUso().EjecutarAsync(1);

            // Assert
            Assert.Equal(EstadoVenta.Anulada, venta.Estado);

            Assert.Equal(10m, producto.StockActual);

            var movimiento = Assert.Single(
                _movimientos.Movimientos);

            Assert.Equal(
                TipoMovimientoStock.AnulacionVenta,
                movimiento.Tipo);

            Assert.Equal(1.5m, movimiento.Cantidad);
            Assert.Equal(8.5m, movimiento.StockAnterior);
            Assert.Equal(10m, movimiento.StockPosterior);

            Assert.True(
                _unitOfWork.GuardarCambiosFueLlamado);
        }


        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task AnularVenta_IdInvalido_NoGuardaCambios(int ventaId)
        {
            await Assert.ThrowsAsync<ArgumentException>(
                () => CrearCasoDeUso().EjecutarAsync(ventaId));

            Assert.Empty(_movimientos.Movimientos);
            Assert.False(_unitOfWork.GuardarCambiosFueLlamado);
        }

        [Fact]
        public async Task AnularVenta_VentaInexistente_NoGuardaCambios()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => CrearCasoDeUso().EjecutarAsync(99));

            Assert.Empty(_movimientos.Movimientos);
            Assert.False(_unitOfWork.GuardarCambiosFueLlamado);
        }

        [Fact]
        public async Task AnularVenta_VentaPendiente_NoGuardaCambios()
        {
            var venta = new Venta(1);

            AsignarId(venta, 1);

            await _ventas.AgregarAsync(venta);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => CrearCasoDeUso().EjecutarAsync(1));

            Assert.Equal(EstadoVenta.Pendiente, venta.Estado);
            Assert.Empty(_movimientos.Movimientos);
            Assert.False(_unitOfWork.GuardarCambiosFueLlamado);
        }

        [Fact]
        public async Task AnularVenta_VentaYaAnulada_NoDevuelveStockDosVeces()
        {
            var categoria = new Categoria(
                "Periféricos",
                "Productos periféricos");

            var producto = new Producto(
                "TEC-002",
                "Mouse",
                8000m,
                5000m,
                2m,
                categoria);

            AsignarId(producto, 1);

            producto.AumentarStock(10m);

            var venta = new Venta(1);

            venta.AgregarDetalle(producto, 2m, 8000m);
            venta.Confirmar();

            AsignarId(venta, 1);

            producto.DisminuirStock(2m);

            _productos.ConfigurarProducto(1, producto);

            await _ventas.AgregarAsync(venta);

            var casoDeUso = CrearCasoDeUso();

            // Primera anulación.
            await casoDeUso.EjecutarAsync(1);

            Assert.Equal(10m, producto.StockActual);
            Assert.Equal(EstadoVenta.Anulada, venta.Estado);
            Assert.Single(_movimientos.Movimientos);

            // Segunda anulación.
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => casoDeUso.EjecutarAsync(1));

            // No debe volver a ingresar stock.
            Assert.Equal(10m, producto.StockActual);
            Assert.Single(_movimientos.Movimientos);
        }

        [Fact]
        public async Task AnularVenta_ConVariosProductos_RestauraTodosLosStocks()
        {
            // Arrange
            var categoria = new Categoria(
                "Periféricos",
                "Productos periféricos");

            var teclado = new Producto(
                "TEC-003", "Teclado", 15000m, 10000m, 2m, categoria);

            var mouse = new Producto(
                "MOU-003", "Mouse", 8000m, 5000m, 2m, categoria);

            AsignarId(teclado, 1);
            AsignarId(mouse, 2);

            teclado.AumentarStock(10m);
            mouse.AumentarStock(8m);

            var venta = new Venta(1);

            venta.AgregarDetalle(teclado, 2m, 15000m);
            venta.AgregarDetalle(mouse, 1.5m, 8000m);
            venta.Confirmar();

            AsignarId(venta, 1);

            // Simulamos el registro original de la venta.
            teclado.DisminuirStock(2m);
            mouse.DisminuirStock(1.5m);

            _productos.ConfigurarProducto(1, teclado);
            _productos.ConfigurarProducto(2, mouse);

            await _ventas.AgregarAsync(venta);

            // Act
            await CrearCasoDeUso().EjecutarAsync(1);

            // Assert
            Assert.Equal(EstadoVenta.Anulada, venta.Estado);

            Assert.Equal(10m, teclado.StockActual);
            Assert.Equal(8m, mouse.StockActual);

            Assert.Equal(2, _movimientos.Movimientos.Count);

            Assert.Contains(_movimientos.Movimientos,
                m => m.Producto == teclado &&
                     m.Tipo == TipoMovimientoStock.AnulacionVenta &&
                     m.Cantidad == 2m);

            Assert.Contains(_movimientos.Movimientos,
                m => m.Producto == mouse &&
                     m.Tipo == TipoMovimientoStock.AnulacionVenta &&
                     m.Cantidad == 1.5m);

            Assert.True(_unitOfWork.GuardarCambiosFueLlamado);
        }


        [Fact]
        public async Task AnularVenta_ProductoInactivo_RestauraStock()
        {
            // Arrange
            var categoria = new Categoria(
                "Periféricos",
                "Productos periféricos");

            var producto = new Producto(
                "TEC-004", "Teclado", 15000m, 10000m, 2m, categoria);

            AsignarId(producto, 1);

            producto.AumentarStock(10m);

            var venta = new Venta(1);

            venta.AgregarDetalle(producto, 3m, 15000m);
            venta.Confirmar();

            AsignarId(venta, 1);

            producto.DisminuirStock(3m);

            // El producto se desactiva después de la venta.
            producto.Desactivar();

            _productos.ConfigurarProducto(1, producto);

            await _ventas.AgregarAsync(venta);

            // Act
            await CrearCasoDeUso().EjecutarAsync(1);

            // Assert
            Assert.Equal(EstadoVenta.Anulada, venta.Estado);
            Assert.Equal(10m, producto.StockActual);
            Assert.Equal(EstadoProducto.Inactivo, producto.Estado);

            var movimiento = Assert.Single(_movimientos.Movimientos);

            Assert.Equal(TipoMovimientoStock.AnulacionVenta, movimiento.Tipo);
            Assert.Equal(3m, movimiento.Cantidad);

            Assert.True(_unitOfWork.GuardarCambiosFueLlamado);
        }


        [Fact]
        public async Task AnularVenta_ProductoInexistente_NoModificaInventario()
        {
            // Arrange
            var categoria = new Categoria(
                "Periféricos",
                "Productos periféricos");

            var producto1 = new Producto(
                "TEC-005", "Teclado", 15000m, 10000m, 2m, categoria);

            var producto2 = new Producto(
                "MOU-005", "Mouse", 8000m, 5000m, 2m, categoria);

            AsignarId(producto1, 1);
            AsignarId(producto2, 2);

            producto1.AumentarStock(10m);
            producto2.AumentarStock(8m);

            var venta = new Venta(1);

            venta.AgregarDetalle(producto1, 2m, 15000m);
            venta.AgregarDetalle(producto2, 1m, 8000m);
            venta.Confirmar();

            AsignarId(venta, 1);

            producto1.DisminuirStock(2m);
            producto2.DisminuirStock(1m);

            // Solo registramos el primer producto en el repositorio.
            _productos.ConfigurarProducto(1, producto1);

            await _ventas.AgregarAsync(venta);

            // Act y Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => CrearCasoDeUso().EjecutarAsync(1));

            Assert.Equal(EstadoVenta.Confirmada, venta.Estado);
            Assert.Equal(8m, producto1.StockActual);

            Assert.Empty(_movimientos.Movimientos);
            Assert.False(_unitOfWork.GuardarCambiosFueLlamado);
        }


        [Fact]
        public async Task AnularVenta_VentaFacturada_NoAnulaNiRestauraStock()
        {
            // Arrange
            var categoria = new Categoria("Componentes");

            var producto = new Producto(
                "RAM-001",
                "Memoria RAM 8 GB",
                15000m,
                10000m,
                2m,
                categoria);

            AsignarId(producto, 1);

            producto.AumentarStock(10m);

            var venta = new Venta(1);

            venta.AgregarDetalle(
                producto,
                cantidad: 2m,
                precioUnitario: 15000m);

            venta.Confirmar();

            AsignarId(venta, 1);

            // Simulamos el descuento original de la venta.
            producto.DisminuirStock(2m);

            _productos.ConfigurarProducto(1, producto);

            await _ventas.AgregarAsync(venta);

            // La venta ya tiene una factura emitida.
            _documentos.Factura = new Factura(
                venta.Id,
                TipoFactura.B,
                venta.Total,
                "INT-FACTURA-001");

            // Act
            var excepcion =
                await Assert.ThrowsAsync<InvalidOperationException>(
                    () => CrearCasoDeUso().EjecutarAsync(venta.Id));

            // Assert
            Assert.Equal(
                "No se puede anular una venta que ya tiene una factura registrada.",
                excepcion.Message);

            // La venta continúa confirmada.
            Assert.Equal(EstadoVenta.Confirmada, venta.Estado);

            // El stock permanece descontado.
            Assert.Equal(8m, producto.StockActual);

            // No se generan movimientos adicionales.
            Assert.Empty(_movimientos.Movimientos);

            // No se solicita guardar ningún cambio.
            Assert.False(_unitOfWork.GuardarCambiosFueLlamado);
        }


    }
}
