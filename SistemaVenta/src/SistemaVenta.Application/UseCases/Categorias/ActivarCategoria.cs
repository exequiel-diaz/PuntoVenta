
using SistemaVenta.Application.Interfaces;

namespace SistemaVenta.Application.UseCases.Categorias
{
    public class ActivarCategoria
    {
        private readonly ICategoriaRepository _categorias;
        private readonly IUnitOfWork _unitOfWork;

        public ActivarCategoria(
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

            categoria.Activar();

            await _unitOfWork.GuardarCambiosAsync();
        }
    }
}
