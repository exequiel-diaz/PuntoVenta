
using SistemaVenta.Application.DTOs.Informes;

namespace SistemaVenta.Application.Interfaces
{
    public interface IInformeVentasRepository
    {
        Task<InformeVentasPorPeriodoDto> ObtenerVentasPorPeriodoAsync(
            DateTime fechaDesde,
            DateTime fechaHasta);

        Task<List<ProductoMasVendidoDto>> ObtenerProductosMasVendidosAsync(
            DateTime fechaDesde,
            DateTime fechaHasta,
            int cantidad);

        Task<List<VentaDiariaDto>> ObtenerVentasDiariasAsync(
            DateTime fechaDesde,
            DateTime fechaHasta);


    }
}
