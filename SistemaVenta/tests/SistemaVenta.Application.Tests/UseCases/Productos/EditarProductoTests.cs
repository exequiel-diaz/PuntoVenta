using SistemaVenta.Application.Tests.Fakes;
using SistemaVenta.Application.UseCases.Productos;
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Application.Tests.UseCases.Productos
{
    public class EditarProductoTests
    {
        [Fact]
        public async Task EjecutarAsync_DatosValidos_DeberiaEditarProducto()
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

            var productoRepository =
                new FakeProductoRepository();

            productoRepository.ConfigurarProducto(
                1,
                producto);

            var categoriaRepository =
                new FakeCategoriaRepository();

            categoriaRepository.ConfigurarCategoria(
                2,
                categoriaNueva);

            var unitOfWork =
                new FakeUnitOfWork();

            var casoDeUso = new EditarProducto(
                productoRepository,
                categoriaRepository,
                unitOfWork);

            await casoDeUso.EjecutarAsync(
                1,
                "TEC-002",
                "Mouse Gamer",
                "Mouse actualizado",
                15000m,
                7000m,
                3m,
                2);

            Assert.Equal("TEC-002", producto.CodigoInterno);
            Assert.Equal("Mouse Gamer", producto.Nombre);
            Assert.Equal(15000m, producto.PrecioVenta);
            Assert.Equal("Mouse actualizado", producto.Descripcion);
            Assert.Equal(7000m, producto.Costo);
            Assert.Equal(3m, producto.StockMinimo);
            Assert.Same(categoriaNueva, producto.Categoria);
            Assert.Equal(10m, producto.StockActual);

            Assert.True(
                unitOfWork.GuardarCambiosFueLlamado);
        }

        [Fact]
        public async Task EjecutarAsync_ProductoInexistente_DeberiaLanzarExcepcion()
        {
            var productoRepository =
                new FakeProductoRepository();

            var categoriaRepository =
                new FakeCategoriaRepository();

            var unitOfWork =
                new FakeUnitOfWork();

            var casoDeUso = new EditarProducto(
                productoRepository,
                categoriaRepository,
                unitOfWork);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => casoDeUso.EjecutarAsync(
                    999,
                    "TEC-001",
                    "Mouse",
                    null,
                    10000m,
                    5000m,
                    5m,
                    1));

            Assert.False(
                unitOfWork.GuardarCambiosFueLlamado);
        }

        [Fact]
        public async Task EjecutarAsync_CategoriaInexistente_DeberiaLanzarExcepcion()
        {
            var categoriaOriginal =
                new Categoria("Periféricos");

            var producto = new Producto(
                "TEC-001",
                "Mouse",
                10000m,
                5000m,
                5m,
                categoriaOriginal);

            var productoRepository =
                new FakeProductoRepository();

            productoRepository.ConfigurarProducto(
                1,
                producto);

            var categoriaRepository =
                new FakeCategoriaRepository();

            var unitOfWork =
                new FakeUnitOfWork();

            var casoDeUso = new EditarProducto(
                productoRepository,
                categoriaRepository,
                unitOfWork);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => casoDeUso.EjecutarAsync(
                    1,
                    "TEC-002",
                    "Mouse Gamer",
                    null,
                    15000m,
                    7000m,
                    5m,
                    999));

            Assert.False(
                unitOfWork.GuardarCambiosFueLlamado);
        }
    
    
    }
}
