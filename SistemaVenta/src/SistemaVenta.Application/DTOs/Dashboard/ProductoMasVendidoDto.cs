
namespace SistemaVenta.Application.DTOs.Dashboard;

public class ProductoMasVendidoDto
{
    public int ProductoId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public decimal CantidadVendida { get; set; }
}
