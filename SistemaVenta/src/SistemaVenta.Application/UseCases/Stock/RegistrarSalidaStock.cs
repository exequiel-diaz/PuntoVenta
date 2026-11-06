
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Application.UseCases.Stock
{
    public class RegistrarSalidaStock
    {
        private readonly IProductoRepository _productos;
        private readonly IMovimientoStockRepository _movimientos;
        private readonly IUnitOfWork _unitOfWork;

        public RegistrarSalidaStock(
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
            decimal cantidad,
            string tipoSalida,
            string motivo)
        {
            if (tipoSalida != "Rotura" &&
                tipoSalida != "Perdida" &&
                tipoSalida != "DevolucionProveedor")
            {
                throw new ArgumentException(
                    "El tipo de salida no es válido.");
            }

            if (string.IsNullOrWhiteSpace(motivo))
                throw new ArgumentException(
                    "Debés indicar el motivo de la salida.");

            motivo = motivo.Trim();

            if (motivo.Length > 400)
                throw new ArgumentException(
                    "El motivo no puede superar los 400 caracteres.");

            var producto = await _productos
                .ObtenerPorIdAsync(productoId);

            if (producto is null)
                throw new InvalidOperationException(
                    "El producto no existe.");

            decimal anterior = producto.StockActual;

            // El dominio valida cantidad, estado y stock suficiente.
            producto.DisminuirStock(cantidad);

            var movimiento = new MovimientoStock(
                producto,
                TipoMovimientoStock.AjusteManual,
                cantidad,
                anterior,
                producto.StockActual,
                $"{tipoSalida}: {motivo}");

            await _movimientos.AgregarAsync(movimiento);
            await _unitOfWork.GuardarCambiosAsync();
        }
    }
}
