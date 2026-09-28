using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Application.UseCases.Productos
{
    public class CrearProducto
    {
        private readonly IProductoRepository _productoRepository;
        private readonly ICategoriaRepository _categoriaRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CrearProducto(
            IProductoRepository productoRepository,
            ICategoriaRepository categoriaRepository,
            IUnitOfWork unitOfWork)
        {
            _productoRepository = productoRepository;
            _categoriaRepository = categoriaRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task EjecutarAsync(
            string codigoInterno,
            string nombre,
            string? descripcion,
            decimal precioVenta,
            decimal costo,
            decimal stockMinimo,
            int categoriaId)
        {
            var categoria =
                await _categoriaRepository.ObtenerPorIdAsync(categoriaId);

            if (categoria is null)
            {
                throw new InvalidOperationException(
                    "La categoría seleccionada no existe.");
            }

            var producto = new Producto(
                codigoInterno,
                nombre,
                precioVenta,
                costo,
                stockMinimo,
                categoria,
                descripcion);

            await _productoRepository.AgregarAsync(producto);

            await _unitOfWork.GuardarCambiosAsync();
        }
    }
}
