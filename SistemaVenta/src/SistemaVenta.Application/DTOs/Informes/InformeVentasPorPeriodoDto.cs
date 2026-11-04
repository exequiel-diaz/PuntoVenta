
namespace SistemaVenta.Application.DTOs.Informes
{
    public class InformeVentasPorPeriodoDto
    {
        public DateTime FechaDesde { get; set; }

        public DateTime FechaHasta { get; set; }

        public int CantidadVentas { get; set; }

        public decimal TotalVendido { get; set; }

        public decimal TicketPromedio { get; set; }
    }
}
