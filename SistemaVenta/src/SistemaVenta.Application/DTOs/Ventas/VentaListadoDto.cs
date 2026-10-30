
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Application.DTOs.Ventas
{
    public class VentaListadoDto
    {
        public int Id { get; set; }

        public int? ClienteId { get; set; }

        public string NombreCliente { get; set; } = string.Empty;

        public DateTime Fecha { get; set; }

        public EstadoVenta Estado { get; set; }

        public decimal Total { get; set; }
    }
}
