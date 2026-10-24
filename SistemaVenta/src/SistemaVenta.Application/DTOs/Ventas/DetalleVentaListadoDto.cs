
namespace SistemaVenta.Application.DTOs.Ventas
{
    public class DetalleVentaListadoDto
    {
        public int ProductoId { get; set; }

        public string NombreProducto { get; set; } = string.Empty;

        public decimal Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }

        public decimal Subtotal { get; set; }
    }
}
