
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Application.UseCases.Ventas
{
    public class AnularVenta
    {
        private readonly IVentaRepository _ventas;
        private readonly IProductoRepository _productos;
        private readonly IMovimientoStockRepository _movimientos;
        private readonly IUnitOfWork _unitOfWork;

        public AnularVenta(
            IVentaRepository ventas,
            IProductoRepository productos,
            IMovimientoStockRepository movimientos,
            IUnitOfWork unitOfWork)
        {
            _ventas = ventas;
            _productos = productos;
            _movimientos = movimientos;
            _unitOfWork = unitOfWork;
        }

        public async Task EjecutarAsync(int ventaId)
        {
            // 1. Validar identificador

            if (ventaId <= 0)
            {
                throw new ArgumentException(
                    "El identificador de la venta debe ser válido.",
                    nameof(ventaId));
            }

            // 2. Recuperar la venta

            var venta = await _ventas.ObtenerPorIdAsync(ventaId);

            if (venta is null)
            {
                throw new InvalidOperationException(
                    "La venta no existe.");
            }

            // 3. Verificar estado

            if (venta.Estado != EstadoVenta.Confirmada)
            {
                throw new InvalidOperationException(
                    "Solo se puede anular una venta confirmada.");
            }

            // 4. Recuperar todos los productos antes de modificarlos

            var productosParaRestaurar =
                new List<(Producto Producto, decimal Cantidad)>();

            foreach (var detalle in venta.Detalles)
            {
                var producto = await _productos.ObtenerPorIdAsync(
                    detalle.ProductoId);

                if (producto is null)
                {
                    throw new InvalidOperationException(
                        $"El producto {detalle.ProductoId} no existe.");
                }

                productosParaRestaurar.Add((
                    producto,
                    detalle.Cantidad));
            }

            // 5. Restaurar stock y registrar movimientos

            foreach (var item in productosParaRestaurar)
            {
                var stockAnterior = item.Producto.StockActual;

                item.Producto.RestaurarStockPorAnulacion(
                    item.Cantidad);

                var stockPosterior = item.Producto.StockActual;

                var movimiento = new MovimientoStock(
                    producto: item.Producto,
                    tipo: TipoMovimientoStock.AnulacionVenta,
                    cantidad: item.Cantidad,
                    stockAnterior: stockAnterior,
                    stockPosterior: stockPosterior,
                    motivo: $"Anulación de venta #{venta.Id}");

                await _movimientos.AgregarAsync(movimiento);
            }

            // 6. Cambiar estado de la venta

            venta.Anular();

            // 7. Persistir todos los cambios

            await _unitOfWork.GuardarCambiosAsync();
        }
    }
}
