using SistemaVenta.Application.Tests.Fakes;
using SistemaVenta.Application.UseCases.Productos;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaVenta.Application.Tests.UseCases.Productos
{
    public class DesactivarProductoTests
    {
        [Fact]
        public async Task EjecutarAsync_ProductoExistente_DeberiaDesactivarlo()
        {
            var categoria = new Categoria("Periféricos");

            var producto = new Producto(
                "TEC-001",
                "Mouse",
                10000m,
                5000m,
                5m,
                categoria);

            var productoRepository = new FakeProductoRepository
            {
                Producto = producto
            };

            var unitOfWork = new FakeUnitOfWork();

            var casoDeUso = new DesactivarProducto(
                productoRepository,
                unitOfWork);

            await casoDeUso.EjecutarAsync(producto.Id);

            Assert.Equal(
                EstadoProducto.Inactivo,
                producto.Estado);

            Assert.True(
                unitOfWork.GuardarCambiosFueLlamado);
        }

        [Fact]
        public async Task EjecutarAsync_ProductoInexistente_DeberiaLanzarExcepcion()
        {
            var productoRepository = new FakeProductoRepository
            {
                Producto = null
            };

            var unitOfWork = new FakeUnitOfWork();

            var casoDeUso = new DesactivarProducto(
                productoRepository,
                unitOfWork);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => casoDeUso.EjecutarAsync(999));

            Assert.False(
                unitOfWork.GuardarCambiosFueLlamado);
        }

    }
}
