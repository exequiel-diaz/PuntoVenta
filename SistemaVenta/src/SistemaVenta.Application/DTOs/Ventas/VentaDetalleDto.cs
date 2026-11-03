
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Application.DTOs.Ventas
{
    public class VentaDetalleDto
    {
        public int Id { get; set; }

        public int? ClienteId { get; set; }

        public string NombreCliente { get; set; } = string.Empty;

        public DateTime Fecha { get; set; }

        public EstadoVenta Estado { get; set; }

        public decimal Total { get; set; }
        public bool TieneFactura { get; set; }
        public int? FacturaId { get; set; }
        public bool EsVentaCuentaCorriente { get; set; }

        public decimal SaldoPendiente { get; set; }

        public List<DetalleVentaListadoDto> Detalles { get; set; } = new();

        public List<NotaComercialDto> NotasComerciales { get; set; } = new();

    }
}
