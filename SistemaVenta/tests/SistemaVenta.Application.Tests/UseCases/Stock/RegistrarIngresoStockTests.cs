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
            var categoria = new Categoria("Monitores");

            var producto = new Producto(
                "MON-001",
                "Monitor",
                250000m,
                180000m,
                5m,
                categoria);

            producto.AumentarStock(5m);

            var productoRepository = 
                new FakeProductoRepository();

            productoRepository.ConfigurarProducto(
                1,
                producto);

            var movimientoRepository =
                new FakeMovimientoStockRepository();

            var unitOfWork=
                new FakeUnitOfWork();

            var casoDeUso = new RegistrarIngresoStock(
                productoRepository,
                movimientoRepository,
                unitOfWork);

            await casoDeUso.EjecutarAsync(
                1,
                10m,
                "Ingreso de mercadería");

            Assert.Equal(15m, producto.StockActual);
            
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
            
            Assert.True(unitOfWork.GuardarCambiosFueLlamado);
            Assert.Same(
                producto,
                movimientoRepository.MovimientoAgregado.Producto);
        }

        [Fact]
        public async Task EjecutarAsync_ProductoInexistente_DebeLanzarExcepcion()
        {
            var productoRepository = 
                new FakeProductoRepository();

            var movimientoRepository =
                new FakeMovimientoStockRepository();

            var unitOfWork =
                new FakeUnitOfWork();

            var casoDeUso = new RegistrarIngresoStock(
                productoRepository,
                movimientoRepository,
                unitOfWork);

            
            var excepcion = await Assert.ThrowsAsync<InvalidOperationException>(
                () => casoDeUso.EjecutarAsync(
                    999,
                    10m,
                    "Ingreso de mercadería"));

            Assert.Equal(
                "El producto no existe.",
                excepcion.Message);

            Assert.Null(movimientoRepository.MovimientoAgregado);
            Assert.False(unitOfWork.GuardarCambiosFueLlamado);
        }

        [Fact]
        public async Task EjecutarAsync_ConCantidadCero_DebeLanzarExcepcion()
        {
            
            var categoria = new Categoria("Monitores");

            var producto = new Producto(
                "MON-001",
                "Monitor",
                250000m,
                180000m,
                5m,
                categoria);

            var productoRepository = 
                new FakeProductoRepository();

            productoRepository.ConfigurarProducto(
                1,
                producto);

            var movimientoRepository =
                new FakeMovimientoStockRepository();

            var unitOfWork =
                new FakeUnitOfWork();

            var casoDeUso = new RegistrarIngresoStock(
                productoRepository,
                movimientoRepository,
                unitOfWork);

            
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => casoDeUso.EjecutarAsync(
                    1,
                    0m,
                    "Ingreso inválido"));

            Assert.Equal(0m, producto.StockActual);
            Assert.Null(movimientoRepository.MovimientoAgregado);
            Assert.False(unitOfWork.GuardarCambiosFueLlamado);
        }

    }
}
