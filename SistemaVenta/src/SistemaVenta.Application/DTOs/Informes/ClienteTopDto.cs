
namespace SistemaVenta.Application.DTOs.Informes
{
    public class ClienteTopDto
    {
        public int ClienteId { get; set; }

        public string NombreCliente { get; set; } = string.Empty;

        public int CantidadVentas { get; set; }

        public decimal TotalComprado { get; set; }

        public decimal TicketPromedio { get; set; }
    }
}
