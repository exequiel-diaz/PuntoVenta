
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Application.Interfaces
{
    public interface IProductoRepository
    {
        Task<Producto?> ObtenerPorIdAsync(int id);

        Task<IReadOnlyList<Producto>> ObtenerTodosAsync();

        Task AgregarAsync(Producto producto);

        Task EliminarAsync(Producto producto);
        Task<bool> ExisteCodigoInternoAsync(string codigoInterno);

    }
}
