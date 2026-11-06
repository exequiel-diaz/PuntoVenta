
using System.ComponentModel.DataAnnotations;

namespace SistemaVenta.Web.Models.Categorias
{
    public class EditarCategoriaViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100)]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }
    }
}
