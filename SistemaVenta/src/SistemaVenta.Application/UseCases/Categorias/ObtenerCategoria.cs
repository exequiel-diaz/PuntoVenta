
using SistemaVenta.Application.DTOs.Categorias;
using SistemaVenta.Application.Interfaces;

namespace SistemaVenta.Application.UseCases.Categorias
{
    public class ObtenerCategoria
    {
        private readonly ICategoriaRepository _categorias;

        public ObtenerCategoria(ICategoriaRepository categorias)
        {
            _categorias = categorias;
        }

        public async Task<CategoriaListadoDto?> EjecutarAsync(int id)
        {
            var categoria =
                await _categorias.ObtenerPorIdAsync(id);

            if (categoria is null)
                return null;

            return new CategoriaListadoDto
            {
                Id = categoria.Id,
                Nombre = categoria.Nombre,
                Descripcion = categoria.Descripcion,
                Activa = categoria.Activa
            };
        }
    }
}
