
namespace SistemaVenta.Application.DTOs.Informes;

public class ProductoRentabilidadDto
{
    public int ProductoId { get; set; }
    public string NombreProducto { get; set; } = string.Empty;

    public decimal UnidadesVendidas { get; set; }
    public decimal Facturacion { get; set; }
    public decimal CostoTotal { get; set; }
    public decimal GananciaBruta { get; set; }

    public decimal MargenBruto =>
        Facturacion > 0
            ? Math.Round(
                GananciaBruta / Facturacion * 100m, 2)
            : 0m;
}
