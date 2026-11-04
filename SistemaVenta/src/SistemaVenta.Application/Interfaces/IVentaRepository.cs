
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Application.Interfaces
{
    public interface IVentaRepository
    {
        Task<Venta?> ObtenerPorIdAsync(int id);

        Task<IReadOnlyList<Venta>> ObtenerTodosAsync();

        Task AgregarAsync(Venta venta);

        Task<IReadOnlyList<Venta>> ObtenerPorPeriodoAsync(
            DateTime desde,
            DateTime hasta);

        Task<bool> TieneVentasPorVendedorAsync(int vendedorId);

        Task<bool> TieneVentasPorProductoAsync(int productoId);
    }
}
