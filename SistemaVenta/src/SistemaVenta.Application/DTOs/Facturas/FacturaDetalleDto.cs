
using SistemaVenta.Application.DTOs.Ventas;
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Application.DTOs.Facturas
{
    public class FacturaDetalleDto
    {
        public int Id { get; set; }

        public int VentaId { get; set; }

        public string Numero { get; set; } = string.Empty;

        public TipoFactura Tipo { get; set; }

        public DateTime FechaEmision { get; set; }

        public EstadoDocumento Estado { get; set; }

        public string NombreCliente { get; set; } = string.Empty;

        public decimal ImporteTotal { get; set; }

        public string? Observaciones { get; set; }

        public List<DetalleVentaListadoDto> Detalles { get; set; } = new();
    }
}
