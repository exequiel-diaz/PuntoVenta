
using SistemaVenta.Application.DTOs.Categorias;
using SistemaVenta.Application.Interfaces;

namespace SistemaVenta.Application.UseCases.Categorias
{
    public class ListarCategorias
    {
        private readonly ICategoriaRepository _categoriaRepository;

        public ListarCategorias(
            ICategoriaRepository categoriaRepository)
        {
            _categoriaRepository = categoriaRepository;
        }

        public async Task<IReadOnlyList<CategoriaListadoDto>> EjecutarAsync()
        {
            var categorias =
                await _categoriaRepository.ObtenerTodasAsync();

            return categorias
                .Select(c => new CategoriaListadoDto
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Descripcion = c.Descripcion,
                    Activa = c.Activa
                })
                .ToList();
        }
    }
}
