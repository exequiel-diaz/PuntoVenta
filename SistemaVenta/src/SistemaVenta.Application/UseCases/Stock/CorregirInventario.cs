
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Application.UseCases.Stock
{
    public class CorregirInventario
    {
        private readonly IProductoRepository _productos;
        private readonly IMovimientoStockRepository _movimientos;
        private readonly IUnitOfWork _unitOfWork;

        public CorregirInventario(
            IProductoRepository productos,
            IMovimientoStockRepository movimientos,
            IUnitOfWork unitOfWork)
        {
            _productos = productos;
            _movimientos = movimientos;
            _unitOfWork = unitOfWork;
        }

        public async Task EjecutarAsync(
            int productoId,
            decimal stockContado,
            string motivo)
        {
            if (stockContado < 0 ||
                stockContado != decimal.Truncate(stockContado))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(stockContado),
                    "El stock contado debe ser un entero no negativo.");
            }

            if (string.IsNullOrWhiteSpace(motivo))
                throw new ArgumentException(
                    "Debés justificar la corrección.");

            motivo = motivo.Trim();

            if (motivo.Length > 500)
                throw new ArgumentException(
                    "El motivo no puede superar los 500 caracteres.");

            var producto = await _productos
                .ObtenerPorIdAsync(productoId);

            if (producto is null)
                throw new InvalidOperationException(
                    "El producto no existe.");

            decimal anterior = producto.StockActual;

            decimal diferencia = stockContado - anterior;

            if (diferencia == 0)
                throw new InvalidOperationException(
                    "El stock contado coincide con el registrado.");

            if (diferencia > 0)
                producto.AumentarStock(diferencia);
            else
                producto.DisminuirStock(-diferencia);

            var movimiento = new MovimientoStock(
                producto,
                TipoMovimientoStock.CorreccionInventario,
                Math.Abs(diferencia),
                anterior,
                producto.StockActual,
                motivo);

            await _movimientos.AgregarAsync(movimiento);
            await _unitOfWork.GuardarCambiosAsync();
        }
    }
}
