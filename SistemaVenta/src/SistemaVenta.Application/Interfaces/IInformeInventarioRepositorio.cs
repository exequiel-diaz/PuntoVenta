
using SistemaVenta.Application.DTOs.Informes;

namespace SistemaVenta.Application.Interfaces;

public interface IInformeInventarioRepository
{
    Task<List<InventarioProductoDto>>
        ObtenerInventarioActualAsync();

    Task<List<MovimientoStockInformeDto>>
        ObtenerMovimientosAsync(
            DateTime fechaDesdeUtc,
            DateTime fechaHastaUtcExclusiva);
}
