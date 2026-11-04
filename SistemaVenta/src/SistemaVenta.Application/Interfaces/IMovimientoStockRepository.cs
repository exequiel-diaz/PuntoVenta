
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Application.Interfaces
{
    public interface IMovimientoStockRepository
    {
        Task AgregarAsync(MovimientoStock movimiento);

        Task<bool> TieneMovimientosPorProductoAsync(int productoId);
    }
}
