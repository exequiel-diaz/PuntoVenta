using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SistemaVenta.Web.Models.Productos
{
    public class CrearProductoViewModel
    {
        [Required(ErrorMessage = "El código es obligatorio.")]
        [StringLength(50)]
        [Display(Name = "Código interno")]
        public string CodigoInterno { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [Range(0, double.MaxValue,
            ErrorMessage = "El precio no puede ser negativo.")]
        [Display(Name = "Precio de venta")]
        public decimal PrecioVenta { get; set; }

        [Range(0, double.MaxValue,
            ErrorMessage = "El costo no puede ser negativo.")]
        public decimal Costo { get; set; }

        [Range(0, double.MaxValue,
            ErrorMessage = "El stock mínimo no puede ser negativo.")]
        [Display(Name = "Stock mínimo")]
        public decimal StockMinimo { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "Debe seleccionar una categoría.")]
        [Display(Name = "Categoría")]
        public int CategoriaId { get; set; }

        public IEnumerable<SelectListItem> Categorias { get; set; }
            = Enumerable.Empty<SelectListItem>();
    }
}
