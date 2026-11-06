
using System.ComponentModel.DataAnnotations;
using SistemaVenta.Web.Validaciones;

namespace SistemaVenta.Web.Models.Productos
{
    public class CorregirInventarioViewModel
    {
        public int ProductoId { get; set; }

        public string NombreProducto { get; set; } = "";

        public decimal StockActual { get; set; }

        [Range(0, double.MaxValue,
            ErrorMessage = "El stock contado no puede ser negativo.")]
        [NumeroEntero(
            ErrorMessage = "El stock contado debe ser entero.")]
        public decimal StockContado { get; set; }

        [Required(ErrorMessage = "El motivo es obligatorio.")]
        [StringLength(500)]
        public string Motivo { get; set; } = "";
    }
}
