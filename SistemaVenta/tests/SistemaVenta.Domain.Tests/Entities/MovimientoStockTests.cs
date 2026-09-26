using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaVenta.Domain.Tests.Entities
{
    public class MovimientoStockTests
    {
        [Fact]
        public void CrearMovimientoStock_ConDatosValidos_DebeCrearseCorrectamente()
        {
            
            var categoria = new Categoria("Monitores");

            var producto = new Producto(
                "MON-001",
                "Monitor",
                250000m,
                180000m,
                5m,
                categoria);

            
            var movimiento = new MovimientoStock(
                producto,
                TipoMovimientoStock.IngresoMercaderia,
                10m,
                0m,
                10m,
                "Ingreso inicial");

            
            Assert.Same(producto, movimiento.Producto);

            Assert.Equal(
                TipoMovimientoStock.IngresoMercaderia,
                movimiento.Tipo);

            Assert.Equal(10m, movimiento.Cantidad);
            Assert.Equal(0m, movimiento.StockAnterior);
            Assert.Equal(10m, movimiento.StockPosterior);
            Assert.Equal("Ingreso inicial", movimiento.Motivo);
        }

        [Fact]
        public void CrearMovimientoStock_SinProducto_DebeLanzarExcepcion()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new MovimientoStock(
                    null!,
                    TipoMovimientoStock.IngresoMercaderia,
                    10m,
                    0m,
                    10m));
        }

        [Fact]
        public void CrearMovimientoStock_ConCantidadCero_DebeLanzarExcepcion()
        {
            var categoria = new Categoria("Monitores");

            var producto = new Producto(
                "MON-001",
                "Monitor",
                250000m,
                180000m,
                5m,
                categoria);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new MovimientoStock(
                    producto,
                    TipoMovimientoStock.IngresoMercaderia,
                    0m,
                    0m,
                    10m));
        }

        [Fact]
        public void CrearMovimientoStock_ConCantidadNegativa_DebeLanzarExcepcion()
        {
            var categoria = new Categoria("Monitores");

            var producto = new Producto(
                "MON-001",
                "Monitor",
                250000m,
                180000m,
                5m,
                categoria);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new MovimientoStock(
                    producto,
                    TipoMovimientoStock.IngresoMercaderia,
                    -10m,
                    0m,
                    10m));
        }

    
    }
}
