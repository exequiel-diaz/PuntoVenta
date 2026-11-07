
using SistemaVenta.Application.DTOs.Informes;
using SistemaVenta.Application.Interfaces;

namespace SistemaVenta.Application.UseCases.Informes;

public class ObtenerInformeMovimientosStock
{
    private readonly IInformeInventarioRepository _repository;

    public ObtenerInformeMovimientosStock(
        IInformeInventarioRepository repository)
    {
        _repository = repository;
    }

    public Task<List<MovimientoStockInformeDto>> EjecutarAsync(
        DateTime fechaDesdeUtc,
        DateTime fechaHastaUtcExclusiva)
    {
        if (fechaDesdeUtc >= fechaHastaUtcExclusiva)
        {
            throw new ArgumentException(
                "El rango de fechas no es válido.");
        }

        return _repository.ObtenerMovimientosAsync(
            fechaDesdeUtc,
            fechaHastaUtcExclusiva);
    }
}
