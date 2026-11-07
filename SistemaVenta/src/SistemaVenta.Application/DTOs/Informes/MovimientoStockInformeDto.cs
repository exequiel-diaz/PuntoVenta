
namespace SistemaVenta.Application.DTOs.Informes;

public class MovimientoStockInformeDto
{
    public int Id { get; set; }

    public DateTime Fecha { get; set; }

    public string CodigoInterno { get; set; } = "";

    public string Producto { get; set; } = "";

    public string Tipo { get; set; } = "";

    public decimal Cantidad { get; set; }

    public decimal StockAnterior { get; set; }

    public decimal StockPosterior { get; set; }

    public string Motivo { get; set; } = "";
}
