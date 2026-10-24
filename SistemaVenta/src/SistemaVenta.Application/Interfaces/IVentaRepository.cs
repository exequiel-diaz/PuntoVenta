
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Application.Interfaces
{
    public interface IVentaRepository
    {
        Task<Venta?> ObtenerPorIdAsync(int id);

        Task<IReadOnlyList<Venta>> ObtenerTodosAsync();

        Task AgregarAsync(Venta venta);
    }
}
