using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SistemaVenta.Application.Interfaces;

namespace SistemaVenta.Application.UseCases.Productos
{
    public class ActivarProducto
    {
        private readonly IProductoRepository _productoRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ActivarProducto(
            IProductoRepository productoRepository,
            IUnitOfWork unitOfWork)
        {
            _productoRepository = productoRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task EjecutarAsync(int productoId)
        {
            var producto =
                await _productoRepository.ObtenerPorIdAsync(productoId);

            if (producto is null)
            {
                throw new InvalidOperationException(
                    "El producto no existe.");
            }

            producto.Activar();

            await _unitOfWork.GuardarCambiosAsync();
        }
    }
}
