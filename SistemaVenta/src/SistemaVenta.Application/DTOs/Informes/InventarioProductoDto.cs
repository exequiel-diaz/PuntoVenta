
namespace SistemaVenta.Application.DTOs.Informes;

public class InventarioProductoDto
{
    public int ProductoId { get; set; }

    public string CodigoInterno { get; set; } = "";

    public string Nombre { get; set; } = "";

    public string Categoria { get; set; } = "";

    public decimal StockActual { get; set; }

    public decimal StockMinimo { get; set; }

    public decimal PrecioVenta { get; set; }

    public decimal ValorPotencialVenta =>
        StockActual * PrecioVenta;

    public string EstadoStock =>
        StockActual == 0
            ? "Sin stock"
            : StockActual <= StockMinimo
                ? "Stock bajo"
                : "Normal";
}
