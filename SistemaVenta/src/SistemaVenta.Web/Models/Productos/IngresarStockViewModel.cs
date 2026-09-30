using System.ComponentModel.DataAnnotations;

namespace SistemaVenta.Web.Models.Productos
{
    public class IngresarStockViewModel
    {
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        /*
        [Range(
           0.001,
           999999999,
           ErrorMessage = "La cantidad debe ser mayor que cero.")]*/
        public decimal Cantidad { get; set; }

        [StringLength(500)]
        public string? Motivo { get; set; }
    }
}
