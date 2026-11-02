using SistemaVenta.Web.Validaciones;
using System.ComponentModel.DataAnnotations;

namespace SistemaVenta.Web.Models.Productos
{
    public class IngresarStockViewModel
    {
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;

        [Range(1, double.MaxValue,
            ErrorMessage = "La cantidad debe ser mayor o igual a 1.")]
        [NumeroEntero(
            ErrorMessage = "La cantidad debe ser un número entero.")]
        public decimal Cantidad { get; set; }

        [StringLength(500)]
        public string? Motivo { get; set; }
    }
}
