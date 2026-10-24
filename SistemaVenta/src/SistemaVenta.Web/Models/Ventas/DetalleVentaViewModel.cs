
using System.ComponentModel.DataAnnotations;

namespace SistemaVenta.Web.ViewModels.Ventas;

public class DetalleVentaViewModel
{
    [Range(1, int.MaxValue,
        ErrorMessage = "Debe seleccionar un producto.")]
    public int ProductoId { get; set; }

    [Range(
    typeof(decimal),
    "0.001",
    "999999999",
    ParseLimitsInInvariantCulture = true,
    ErrorMessage = "La cantidad debe ser mayor que cero.")]
    public decimal Cantidad { get; set; }
}
