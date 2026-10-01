using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SistemaVenta.Application.DTOs.Productos;
using SistemaVenta.Application.Interfaces;

namespace SistemaVenta.Application.UseCases.Productos
{
    public class ObtenerProducto
    {
        private readonly IProductoRepository _productoRepository;

        public ObtenerProducto(
            IProductoRepository productoRepository)
        {
            _productoRepository = productoRepository;
        }

        public async Task<ProductoDetalleDto?> EjecutarAsync(int id)
        {
            var producto =
                await _productoRepository.ObtenerPorIdAsync(id);

            if (producto is null)
                return null;

            return new ProductoDetalleDto
            {
                Id = producto.Id,
                CodigoInterno = producto.CodigoInterno,
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion,
                PrecioVenta = producto.PrecioVenta,
                Costo = producto.Costo,
                StockActual = producto.StockActual,
                StockMinimo = producto.StockMinimo,
                CategoriaId = producto.Categoria.Id
            };
        }


    }
}
