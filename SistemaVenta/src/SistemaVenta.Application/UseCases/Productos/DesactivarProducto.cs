using SistemaVenta.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaVenta.Application.UseCases.Productos
{
    public class DesactivarProducto
    {
        private readonly IProductoRepository _productoRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DesactivarProducto(
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

            producto.Desactivar();

            await _unitOfWork.GuardarCambiosAsync();
        }
    }
}
