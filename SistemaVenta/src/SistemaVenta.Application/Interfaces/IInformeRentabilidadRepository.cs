
using SistemaVenta.Application.DTOs.Informes;

namespace SistemaVenta.Application.Interfaces;

public interface IInformeRentabilidadRepository
{
    Task<InformeRentabilidadDto> ObtenerAsync(
        DateTime fechaDesde,
        DateTime fechaHasta,
        int top);
}
