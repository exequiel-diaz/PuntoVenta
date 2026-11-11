
using SistemaVenta.Domain.Entities;
using SistemaVenta.Application.DTOs.Dashboard;

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

        Task<IReadOnlyList<ProductoMasVendidoDto>>
            ObtenerProductosMasVendidosAsync(
                DateTime desde,
                DateTime hasta,
                int cantidad = 5);

        Task<bool> TieneVentasPorVendedorAsync(int vendedorId);

        Task<bool> TieneVentasPorProductoAsync(int productoId);
    }
}
