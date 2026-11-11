
namespace SistemaVenta.Application.DTOs.Dashboard;

public class ResumenDashboardDto
{
    public decimal VentasHoy { get; set; }

    public int CantidadVentasHoy { get; set; }

    public int ProductosActivos { get; set; }

    public int ProductosStockBajo { get; set; }

    public int ProductosSinStock { get; set; }

    public List<VentasPorDiaDto> VentasUltimosDias { get; set; }
        = new();

    public List<ProductoMasVendidoDto> ProductosMasVendidos { get; set; }
    = new();
}

public class VentasPorDiaDto
{
    public string Dia { get; set; } = string.Empty;

    public decimal Total { get; set; }
}
