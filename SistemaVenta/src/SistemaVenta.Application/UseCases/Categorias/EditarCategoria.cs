
using SistemaVenta.Application.Interfaces;

namespace SistemaVenta.Application.UseCases.Categorias
{
    public class EditarCategoria
    {
        private readonly ICategoriaRepository _categorias;
        private readonly IUnitOfWork _unitOfWork;

        public EditarCategoria(
            ICategoriaRepository categorias,
            IUnitOfWork unitOfWork)
        {
            _categorias = categorias;
            _unitOfWork = unitOfWork;
        }

        public async Task EjecutarAsync(
            int categoriaId,
            string nombre,
            string? descripcion = null)
        {
            var categoria =
                await _categorias.ObtenerPorIdAsync(categoriaId);

            if (categoria is null)
            {
                throw new InvalidOperationException(
                    "La categoría no existe.");
            }

            categoria.Actualizar(nombre, descripcion);

            await _unitOfWork.GuardarCambiosAsync();
        }
    }
}
