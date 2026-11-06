
namespace SistemaVenta.Application.DTOs.Productos
{
    public class ProductoListadoDto
    {
        public int Id { get; init; }

        public string CodigoInterno { get; init; } = string.Empty;

        public string Nombre { get; init; } = string.Empty;

        public string Categoria { get; init; } = string.Empty;

        public decimal PrecioVenta { get; init; }

        public decimal StockActual { get; init; }
        public decimal StockMinimo { get; init; }

        public string EstadoStock { get; init; } = string.Empty;

        public string Estado { get; init; } = string.Empty;
    }
}
