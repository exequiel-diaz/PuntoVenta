
using SistemaVenta.Application.Interfaces;

namespace SistemaVenta.Application.UseCases.Categorias
{
    public class DesactivarCategoria
    {
        private readonly ICategoriaRepository _categorias;
        private readonly IUnitOfWork _unitOfWork;

        public DesactivarCategoria(
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

            categoria.Desactivar();

            await _unitOfWork.GuardarCambiosAsync();
        }
    }
}
