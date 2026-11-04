
namespace SistemaVenta.Application.DTOs.Informes
{
    public class VentaDiariaDto
    {
        public DateTime Fecha { get; set; }

        public int CantidadVentas { get; set; }

        public decimal TotalVendido { get; set; }
    }
}
