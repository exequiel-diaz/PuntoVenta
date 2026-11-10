
namespace SistemaVenta.Application.DTOs.Informes;

public class InformeRentabilidadDto
{
    public int CantidadVentas { get; set; }
    public int VentasConCosto { get; set; }
    public int VentasSinCosto { get; set; }

    public decimal FacturacionTotal { get; set; }

    // Solo operaciones con costo histórico completo.
    public decimal FacturacionAnalizada { get; set; }
    public decimal CostoMercaderia { get; set; }
    public decimal GananciaBruta { get; set; }

    public decimal MargenBruto =>
        FacturacionAnalizada > 0
            ? Math.Round(
                GananciaBruta / FacturacionAnalizada * 100m, 2)
            : 0m;

    public List<ProductoRentabilidadDto> Productos { get; set; }
        = new();


    public List<RentabilidadDiariaDto> EvolucionDiaria { get; set; }
        = new();

}
