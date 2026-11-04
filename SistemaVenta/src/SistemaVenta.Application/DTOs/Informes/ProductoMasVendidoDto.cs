
namespace SistemaVenta.Application.DTOs.Informes
{
    public class ProductoMasVendidoDto
    {
        public int ProductoId { get; set; }

        public string NombreProducto { get; set; } = string.Empty;

        public decimal UnidadesVendidas { get; set; }

        public decimal TotalVendido { get; set; }
    }
}
