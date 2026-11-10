
namespace SistemaVenta.Application.DTOs.Informes;

public class RentabilidadDiariaDto
{
    public DateTime Fecha { get; set; }

    public decimal Facturacion { get; set; }

    public decimal Costo { get; set; }

    public decimal GananciaBruta => Facturacion - Costo;
}
