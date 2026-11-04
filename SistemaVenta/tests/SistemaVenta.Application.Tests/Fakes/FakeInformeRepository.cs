
using SistemaVenta.Application.DTOs.Informes;
using SistemaVenta.Application.Interfaces;

namespace SistemaVenta.Application.Tests.Fakes
{
    public class FakeInformeVentasRepository : IInformeVentasRepository
    {
        public List<VentaDiariaDto> VentasDiarias { get; set; }
            = new();

        public int CantidadConsultas { get; private set; }

        public Task<List<VentaDiariaDto>> ObtenerVentasDiariasAsync(
            DateTime fechaDesde,
            DateTime fechaHasta)
        {
            CantidadConsultas++;

            return Task.FromResult(VentasDiarias);
        }

        public Task<InformeVentasPorPeriodoDto> ObtenerVentasPorPeriodoAsync(
            DateTime fechaDesde,
            DateTime fechaHasta)
        {
            throw new NotImplementedException();
        }

        public Task<List<ProductoMasVendidoDto>> ObtenerProductosMasVendidosAsync(
            DateTime fechaDesde,
            DateTime fechaHasta,
            int cantidad)
        {
            throw new NotImplementedException();
        }
    }
}
