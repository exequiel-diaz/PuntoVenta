using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Domain.Tests.Entities
{
    public class ProductoTests
    {
        [Fact]
        public void CrearProducto_ConDatosValidos_DebeCrearseCorrectamente()
        {
            //arrange= se preparan los datos
            string codigo = "MON-001";
            string nombre = "Monitor Samsung 24";
            decimal precio = 250000m;
            decimal costo = 180000m;
            decimal stockMinimo = 5m;

            var categoria = new Categoria("Monitores");

            //act= se ejecut lo que queremos probar
            var producto = new Producto(
                codigo,
                nombre,
                precio,
                costo,
                stockMinimo,
                categoria);

            //assert comprueba resultado
            Assert.Equal("MON-001", producto.CodigoInterno);
            Assert.Equal("Monitor Samsung 24", producto.Nombre);
            Assert.Equal(250000m, producto.PrecioVenta);
            Assert.Equal(180000m, producto.Costo);
            Assert.Equal(5m, producto.StockMinimo);

            Assert.Equal(0m, producto.StockActual);
            Assert.Equal(EstadoProducto.Activo, producto.Estado);

            Assert.Same(categoria, producto.Categoria);
        }

        [Fact]
        public void CrearProducto_SinNombre_DebeLanzarExcepcion()
        {
            var categoria = new Categoria("Monitores");
            Assert.Throws<ArgumentException>(() =>
                new Producto(
                    "MON-001",
                    "",
                    250000m,
                    180000m,
                    5m,
                    categoria));
        }

        [Fact]
        public void CrearProducto_SinCodigoInterno_DebeLanzarExcepcion()
        {
            var categoria = new Categoria("Monitores");
            Assert.Throws<ArgumentException>(() =>
                new Producto(
                    "",
                    "Monitor",
                    250000m,
                    180000m,
                    5m,
                    categoria));
        }

        [Fact]
        public void CrearProducto_ConPrecioNegativo_DebeLanzarExcepcion()
        {
            var categoria = new Categoria("Monitores");
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new Producto(
                    "MON-001",
                    "Monitor",
                    -1m,
                    180000m,
                    5m,
                    categoria));
        }

        [Fact]
        public void CrearProducto_ConCostoNegativo_DebeLanzarExcepcion()
        {
            var categoria = new Categoria("Monitores");
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new Producto(
                    "MON-001",
                    "Monitor",
                    250000m,
                    -1m,
                    5m,
                    categoria));
        }

        [Fact]
        public void CrearProducto_ConStockMinimoNegativo_DebeLanzarExcepcion()
        {
            var categoria = new Categoria("Monitores");
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new Producto(
                    "MON-001",
                    "Monitor",
                    250000m,
                    180000m,
                    -1m,
                    categoria));
        }

        [Fact]
        public void CrearProducto_ConEspaciosEnNombreYCodigo_DebeEliminarEspaciosExternos()
        {
            var categoria = new Categoria("Monitores");
            var producto = new Producto(
                "  MON-001  ",
                "  Monitor Samsung 24  ",
                250000m,
                180000m,
                5m,
                categoria);

            Assert.Equal("MON-001", producto.CodigoInterno);
            Assert.Equal("Monitor Samsung 24", producto.Nombre);
        }

        [Fact]
        public void CrearProducto_SinCategoria_DebeLanzarExcepcion()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new Producto(
                    "MON-001",
                    "Monitor",
                    250000m,
                    180000m,
                    5m,
                    null!));
        }
    }
}
