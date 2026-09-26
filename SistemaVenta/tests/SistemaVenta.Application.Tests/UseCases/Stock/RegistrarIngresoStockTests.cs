using SistemaVenta.Application.Tests.Fakes;
using SistemaVenta.Application.UseCases.Stock;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaVenta.Application.Tests.UseCases.Stock
{
    public class RegistrarIngresoStockTests
    {
        [Fact]
        public async Task EjecutarAsync_ConDatosValidos_DebeActualizarStockYRegistrarMovimiento()
        {
            // Arrange
            var categoria = new Categoria("Monitores");

            var producto = new Producto(
                "MON-001",
                "Monitor",
                250000m,
                180000m,
                5m,
                categoria);

            producto.AumentarStock(5m);

            var productoRepository = new FakeProductoRepository
            {
                Producto = producto
            };

            var movimientoRepository =
                new FakeMovimientoStockRepository();

            var casoDeUso = new RegistrarIngresoStock(
                productoRepository,
                movimientoRepository);

            // Act
            await casoDeUso.EjecutarAsync(
                producto.Id,
                10m,
                "Ingreso de mercadería");

            // Assert
            Assert.Equal(15m, producto.StockActual);

            Assert.Same(
                producto,
                productoRepository.ProductoActualizado);

            Assert.NotNull(
                movimientoRepository.MovimientoAgregado);

            Assert.Equal(
                TipoMovimientoStock.IngresoMercaderia,
                movimientoRepository.MovimientoAgregado.Tipo);

            Assert.Equal(
                10m,
                movimientoRepository.MovimientoAgregado.Cantidad);

            Assert.Equal(
                5m,
                movimientoRepository.MovimientoAgregado.StockAnterior);

            Assert.Equal(
                15m,
                movimientoRepository.MovimientoAgregado.StockPosterior);
        }
    }
}
