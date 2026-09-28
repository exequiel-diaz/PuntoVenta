using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SistemaVenta.Application.DTOs.Productos;
using SistemaVenta.Application.Interfaces;

namespace SistemaVenta.Application.UseCases.Productos
{
    public class ListarProductos
    {
        private readonly IProductoRepository _productoRepository;

        public ListarProductos(
            IProductoRepository productoRepository)
        {
            _productoRepository = productoRepository;
        }

        public async Task<IReadOnlyList<ProductoListadoDto>>
            EjecutarAsync()
        {
            var productos =
                await _productoRepository.ObtenerTodosAsync();

            return productos
                .Select(p => new ProductoListadoDto
                {
                    Id = p.Id,
                    CodigoInterno = p.CodigoInterno,
                    Nombre = p.Nombre,
                    Categoria = p.Categoria.Nombre,
                    PrecioVenta = p.PrecioVenta,
                    StockActual = p.StockActual,
                    Estado = p.Estado.ToString()
                })
                .ToList();
        }
    }
}
