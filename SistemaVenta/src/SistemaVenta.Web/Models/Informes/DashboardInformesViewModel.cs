
using SistemaVenta.Application.DTOs.Informes;

namespace SistemaVenta.Web.Models.Informes
{
    public class DashboardInformesViewModel
    {
        public InformeVentasPorPeriodoDto ResumenVentas { get; set; }
            = new();

        public List<VentaDiariaDto> VentasDiarias { get; set; }
            = new();

        public List<VentaPorVendedorDto> VentasPorVendedor { get; set; }
            = new();

        public List<ProductoMasVendidoDto> ProductosDestacados { get; set; }
            = new();

        public List<ClienteTopDto> MejoresClientes { get; set; }
            = new();

        public DateTime FechaDesde { get; set; }

        public DateTime FechaHasta { get; set; }
    }
}
