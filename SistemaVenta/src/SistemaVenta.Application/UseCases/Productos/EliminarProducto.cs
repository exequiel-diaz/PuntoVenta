
using SistemaVenta.Application.Interfaces;

namespace SistemaVenta.Application.UseCases.Productos
{
    public class EliminarProducto
    {
        private readonly IProductoRepository _productoRepository;
        private readonly IVentaRepository _ventaRepository;
        private readonly IMovimientoStockRepository _movimientoStockRepository;
        private readonly IUnitOfWork _unitOfWork;

        public EliminarProducto(
            IProductoRepository productoRepository,
            IVentaRepository ventaRepository,
            IMovimientoStockRepository movimientoStockRepository,
            IUnitOfWork unitOfWork)
        {
            _productoRepository = productoRepository;
            _ventaRepository = ventaRepository;
            _movimientoStockRepository = movimientoStockRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task EjecutarAsync(int id)
        {
            var producto = await _productoRepository.ObtenerPorIdAsync(id);

            if (producto is null)
            {
                throw new InvalidOperationException(
                    "El producto no existe.");
            }

            bool tieneVentas =
                await _ventaRepository.TieneVentasPorProductoAsync(id);

            if (tieneVentas)
            {
                throw new InvalidOperationException(
                    "No se puede eliminar el producto porque tiene ventas registradas.");
            }

            bool tieneMovimientos =
                await _movimientoStockRepository.TieneMovimientosPorProductoAsync(id);

            if (tieneMovimientos)
            {
                throw new InvalidOperationException(
                    "No se puede eliminar el producto porque tiene movimientos de stock registrados.");
            }

            await _productoRepository.EliminarAsync(producto);

            await _unitOfWork.GuardarCambiosAsync();
        }
    }
}
