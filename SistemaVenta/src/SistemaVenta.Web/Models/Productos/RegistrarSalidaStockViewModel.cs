
using System.ComponentModel.DataAnnotations;
using SistemaVenta.Web.Validaciones;

namespace SistemaVenta.Web.Models.Productos
{
    public class RegistrarSalidaStockViewModel
    {
        public int ProductoId { get; set; }

        public string NombreProducto { get; set; } = "";

        public decimal StockActual { get; set; }

        [Required(ErrorMessage = "Seleccioná un tipo de salida.")]
        public string TipoSalida { get; set; } = "";

        [Range(1, double.MaxValue,
            ErrorMessage = "La cantidad debe ser mayor que cero.")]
        [NumeroEntero(
            ErrorMessage = "La cantidad debe ser un número entero.")]
        public decimal Cantidad { get; set; }

        [Required(ErrorMessage = "El motivo es obligatorio.")]
        [StringLength(400)]
        public string Motivo { get; set; } = "";
    }
}
