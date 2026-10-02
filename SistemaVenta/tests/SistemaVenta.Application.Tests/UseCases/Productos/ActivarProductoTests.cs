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
    public class ActivarProductoTests
    {
        [Fact]
        public async Task EjecutarAsync_ProductoInactivo_DeberiaActivarlo()
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

            var productoRepository = new FakeProductoRepository();

            productoRepository.ConfigurarProducto(
                1,
                producto);
            
            var unitOfWork = new FakeUnitOfWork();

            var casoDeUso = new ActivarProducto(
                productoRepository,
                unitOfWork);

            await casoDeUso.EjecutarAsync(1);

            Assert.Equal(
                EstadoProducto.Activo,
                producto.Estado);

            Assert.True(
                unitOfWork.GuardarCambiosFueLlamado);
        }

        [Fact]
        public async Task EjecutarAsync_ProductoInexistente_DeberiaLanzarExcepcion()
        {
            var productoRepository = new FakeProductoRepository();

            var unitOfWork = new FakeUnitOfWork();

            var casoDeUso = new ActivarProducto(
                productoRepository,
                unitOfWork);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => casoDeUso.EjecutarAsync(999));

            Assert.False(
                unitOfWork.GuardarCambiosFueLlamado);
        }
    }
}
