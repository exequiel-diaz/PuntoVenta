
using SistemaVenta.Application.DTOs.Informes;
using SistemaVenta.Application.Interfaces;

namespace SistemaVenta.Application.UseCases.Informes;

public class ObtenerInformeInventario
{
    private readonly IInformeInventarioRepository _repository;

    public ObtenerInformeInventario(
        IInformeInventarioRepository repository)
    {
        _repository = repository;
    }

    public Task<List<InventarioProductoDto>> EjecutarAsync()
    {
        return _repository.ObtenerInventarioActualAsync();
    }
}
