using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaVenta.Application.UseCases.Stock
{
    public class RegistrarIngresoStock
    {
        private readonly IProductoRepository _productoRepository;
        private readonly IMovimientoStockRepository _movimientoStockRepository;
        private readonly IUnitOfWork _unitOfWork;
        public RegistrarIngresoStock(
            IProductoRepository productoRepository,
            IMovimientoStockRepository movimientoStockRepository,
            IUnitOfWork unitOfWork)
        {
            _productoRepository = productoRepository;
            _movimientoStockRepository = movimientoStockRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task EjecutarAsync(
            int productoId,
            decimal cantidad,
            string? motivo = null)
        {
            var producto =
                await _productoRepository.ObtenerPorIdAsync(productoId);

            if (producto is null)
            {
                throw new InvalidOperationException(
                    "El producto no existe.");
            }

            decimal stockAnterior = producto.StockActual;

            producto.AumentarStock(cantidad);

            decimal stockPosterior = producto.StockActual;

            var movimiento = new MovimientoStock(
                producto,
                TipoMovimientoStock.IngresoMercaderia,
                cantidad,
                stockAnterior,
                stockPosterior,
                motivo);

            //await _productoRepository.ActualizarAsync(producto);
            await _movimientoStockRepository.AgregarAsync(movimiento);
            await _unitOfWork.GuardarCambiosAsync();
            
        }
    }
}
