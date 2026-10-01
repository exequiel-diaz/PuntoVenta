using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SistemaVenta.Application.Interfaces;

namespace SistemaVenta.Application.UseCases.Productos
{
    public class EditarProducto
    {
        private readonly IProductoRepository _productoRepository;
        private readonly ICategoriaRepository _categoriaRepository;
        private readonly IUnitOfWork _unitOfWork;

        public EditarProducto(
            IProductoRepository productoRepository,
            ICategoriaRepository categoriaRepository,
            IUnitOfWork unitOfWork)
        {
            _productoRepository = productoRepository;
            _categoriaRepository = categoriaRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task EjecutarAsync(
            int productoId,
            string codigoInterno,
            string nombre,
            string? descripcion,
            decimal precioVenta,
            decimal costo,
            decimal stockMinimo,
            int categoriaId)
        {
            var producto =
                await _productoRepository.ObtenerPorIdAsync(productoId);

            if (producto is null)
                throw new InvalidOperationException(
                    "El producto no existe.");

            var categoria =
                await _categoriaRepository.ObtenerPorIdAsync(categoriaId);

            if (categoria is null)
                throw new InvalidOperationException(
                    "La categoría seleccionada no existe.");

            producto.ActualizarDatos(
                codigoInterno,
                nombre,
                precioVenta,
                costo,
                stockMinimo,
                categoria,
                descripcion);

            await _unitOfWork.GuardarCambiosAsync();
        }
    }
}
