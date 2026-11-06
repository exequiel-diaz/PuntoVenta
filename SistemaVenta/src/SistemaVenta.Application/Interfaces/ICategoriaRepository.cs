
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Application.Interfaces
{
    public interface ICategoriaRepository
    {
        Task<IReadOnlyList<Categoria>> ObtenerTodasAsync();

        Task<Categoria?> ObtenerPorIdAsync(int id);

        Task AgregarAsync(Categoria categoria);

        Task EliminarAsync(Categoria categoria);

        Task<bool> TieneProductosAsync(int categoriaId);
    }
}
