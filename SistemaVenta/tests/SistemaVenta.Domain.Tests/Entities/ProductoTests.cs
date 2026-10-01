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

        [Fact]
        public void AumentarStock_ConCantidadValida_DebeIncrementarStock()
        {
            var categoria = new Categoria("Monitores");

            var producto = new Producto(
                "MON-001",
                "Monitor",
                250000m,
                180000m,
                5m,
                categoria);

            producto.AumentarStock(10m);

            Assert.Equal(10m, producto.StockActual);
        }

        [Fact]
        public void AumentarStock_ConCantidadNegativa_DebeLanzarExcepcion()
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
                producto.AumentarStock(-10m));
        }

        [Fact]
        public void AumentarStock_ConCantidadCero_DebeLanzarExcepcion()
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
                producto.AumentarStock(0m));
        }

        [Fact]
        public void DisminuirStock_ConStockSuficiente_DebeReducirStock()
        {
            var categoria = new Categoria("Monitores");

            var producto = new Producto(
                "MON-001",
                "Monitor",
                250000m,
                180000m,
                5m,
                categoria);

            producto.AumentarStock(10m);

            producto.DisminuirStock(4m);

            Assert.Equal(6m, producto.StockActual);
        }

        [Fact]
        public void DisminuirStock_SinStockSuficiente_DebeLanzarExcepcion()
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

            Assert.Throws<InvalidOperationException>(() =>
                producto.DisminuirStock(8m));

            Assert.Equal(5m, producto.StockActual);
        }

        [Fact]
        public void ObtenerEstadoStock_SinStock_DebeRetornarSinStock()
        {
            var categoria = new Categoria("Monitores");

            var producto = new Producto(
                "MON-001",
                "Monitor",
                250000m,
                180000m,
                5m,
                categoria);

            Assert.Equal(
                EstadoStock.SinStock,
                producto.ObtenerEstadoStock());
        }

        [Fact]
        public void ObtenerEstadoStock_ConStockIgualAlMinimo_DebeRetornarStockBajo()
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

            Assert.Equal(
                EstadoStock.StockBajo,
                producto.ObtenerEstadoStock());
        }

        [Fact]
        public void ObtenerEstadoStock_ConStockSuperiorAlMinimo_DebeRetornarNormal()
        {
            var categoria = new Categoria("Monitores");

            var producto = new Producto(
                "MON-001",
                "Monitor",
                250000m,
                180000m,
                5m,
                categoria);

            producto.AumentarStock(10m);

            Assert.Equal(
                EstadoStock.Normal,
                producto.ObtenerEstadoStock());
        }

        [Fact]
        public void Desactivar_DeberiaCambiarEstadoAInactivo()
        {
            var categoria = new Categoria("Periféricos");

            var producto = new Producto(
                "TEC-001",
                "Mouse",
                10000m,
                5000m,
                5m,
                categoria);

            producto.Desactivar();

            Assert.Equal(
                EstadoProducto.Inactivo,
                producto.Estado);
        }

        [Fact]
        public void Activar_ProductoInactivo_DeberiaCambiarEstadoAActivo()
        {
            var categoria = new Categoria("Periféricos");

            var producto = new Producto(
                "TEC-001",
                "Mouse",
                10000m,
                5000m,
                5m,
                categoria);

            producto.Desactivar();

            producto.Activar();

            Assert.Equal(
                EstadoProducto.Activo,
                producto.Estado);
        }

        [Fact]
        public void AumentarStock_ProductoInactivo_DeberiaLanzarExcepcion()
        {
            var categoria = new Categoria("Periféricos");

            var producto = new Producto(
                "TEC-001",
                "Mouse",
                10000m,
                5000m,
                5m,
                categoria);

            producto.Desactivar();

            Assert.Throws<InvalidOperationException>(
                () => producto.AumentarStock(10m));

            Assert.Equal(0m, producto.StockActual);
        }

        [Fact]
        public void ActualizarDatos_DatosValidos_DeberiaModificarProducto()
        {
            var categoriaOriginal =
                new Categoria("Periféricos");

            var categoriaNueva =
                new Categoria("Accesorios");

            var producto = new Producto(
                "TEC-001",
                "Mouse",
                10000m,
                5000m,
                5m,
                categoriaOriginal);

            producto.AumentarStock(10m);

            producto.ActualizarDatos(
                "TEC-002",
                "Mouse Gamer",
                15000m,
                7000m,
                3m,
                categoriaNueva,
                "Mouse para gaming");

            Assert.Equal("TEC-002", producto.CodigoInterno);
            Assert.Equal("Mouse Gamer", producto.Nombre);
            Assert.Equal("Mouse para gaming", producto.Descripcion);
            Assert.Equal(15000m, producto.PrecioVenta);
            Assert.Equal(7000m, producto.Costo);
            Assert.Equal(3m, producto.StockMinimo);
            Assert.Same(categoriaNueva, producto.Categoria);

            Assert.Equal(10m, producto.StockActual);
        }

        [Fact]
        public void ActualizarDatos_NombreVacio_DeberiaLanzarExcepcion()
        {
            var categoria = new Categoria("Periféricos");

            var producto = new Producto(
                "TEC-001",
                "Mouse",
                10000m,
                5000m,
                5m,
                categoria);

            Assert.Throws<ArgumentException>(() =>
                producto.ActualizarDatos(
                    "TEC-001",
                    "",
                    10000m,
                    5000m,
                    5m,
                    categoria));
        }
    
    
    }
}
