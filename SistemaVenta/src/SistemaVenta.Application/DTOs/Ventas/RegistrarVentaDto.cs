
namespace SistemaVenta.Application.DTOs.Ventas
{
    public class RegistrarVentaDto
    {
        public int ClienteId { get; set; }

        public List<DetalleVentaEntradaDto> Detalles { get; set; }
            = new();
    }

    public class DetalleVentaEntradaDto
    {
        public int ProductoId { get; set; }

        public decimal Cantidad { get; set; }
    }
}
