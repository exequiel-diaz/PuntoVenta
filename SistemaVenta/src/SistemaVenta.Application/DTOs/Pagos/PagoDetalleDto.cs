
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Application.DTOs.Pagos
{
    public class PagoDetalleDto
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; }

        public decimal Importe { get; set; }

        public FormaPago FormaPago { get; set; }

        public EstadoPago Estado { get; set; }

        public string? Observaciones { get; set; }

        public List<PagoAplicacionDto> Aplicaciones { get; set; } = new();
    }

    public class PagoAplicacionDto
    {
        public string Tipo { get; set; } = string.Empty;

        public int ReferenciaId { get; set; }

        public decimal ImporteAplicado { get; set; }
    }
}
