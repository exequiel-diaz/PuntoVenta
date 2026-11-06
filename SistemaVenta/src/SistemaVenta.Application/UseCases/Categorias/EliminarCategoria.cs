
using SistemaVenta.Application.Interfaces;

namespace SistemaVenta.Application.UseCases.Categorias
{
    public class EliminarCategoria
    {
        private readonly ICategoriaRepository _categorias;
        private readonly IUnitOfWork _unitOfWork;

        public EliminarCategoria(
            ICategoriaRepository categorias,
            IUnitOfWork unitOfWork)
        {
            _categorias = categorias;
            _unitOfWork = unitOfWork;
        }

        public async Task EjecutarAsync(int categoriaId)
        {
            var categoria =
                await _categorias.ObtenerPorIdAsync(categoriaId);

            if (categoria is null)
            {
                throw new InvalidOperationException(
                    "La categoría no existe.");
            }

            var tieneProductos =
                await _categorias.TieneProductosAsync(categoriaId);

            if (tieneProductos)
            {
                throw new InvalidOperationException(
                    "No se puede eliminar una categoría " +
                    "que tiene productos asociados.");
            }

            await _categorias.EliminarAsync(categoria);

            await _unitOfWork.GuardarCambiosAsync();
        }
    }
}
