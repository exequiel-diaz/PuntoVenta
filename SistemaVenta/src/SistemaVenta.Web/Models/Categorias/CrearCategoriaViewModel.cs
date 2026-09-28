using System.ComponentModel.DataAnnotations;

namespace SistemaVenta.Web.Models.Categorias
{
    public class CrearCategoriaViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Descripcion { get; set; }
    }
}
