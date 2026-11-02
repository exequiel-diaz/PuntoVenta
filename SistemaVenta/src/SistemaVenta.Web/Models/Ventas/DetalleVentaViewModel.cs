using System.ComponentModel.DataAnnotations;
using SistemaVenta.Web.Validaciones;

namespace SistemaVenta.Web.ViewModels.Ventas;

public class DetalleVentaViewModel
{
    [Range(1, int.MaxValue,
        ErrorMessage = "Debe seleccionar un producto.")]
    public int ProductoId { get; set; }

    [Range(
        typeof(decimal),
        "1",
        "999999999",
        ParseLimitsInInvariantCulture = true,
        ErrorMessage = "La cantidad debe ser mayor o igual a 1.")]
    [NumeroEntero(
        ErrorMessage = "La cantidad debe ser un número entero.")]
    public decimal Cantidad { get; set; }
}