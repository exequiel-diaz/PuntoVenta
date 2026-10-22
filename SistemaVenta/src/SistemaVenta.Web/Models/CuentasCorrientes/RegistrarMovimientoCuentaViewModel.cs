
using System.ComponentModel.DataAnnotations;

namespace SistemaVenta.Web.ViewModels.CuentasCorrientes;

public class RegistrarMovimientoCuentaViewModel
{
    [Range(1, int.MaxValue)]
    public int CuentaId { get; set; }

    [Range(1, int.MaxValue)]
    public int ClienteId { get; set; }

    [Required]
    public string TipoMovimiento { get; set; } = string.Empty;

    [Display(Name = "Importe")]
    [Range(
        0.01,
        9999999999999999.99,
        ErrorMessage = "El importe debe ser mayor que cero.")]
    public decimal Importe { get; set; }

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(250)]
    [Display(Name = "Descripción")]
    public string Descripcion { get; set; } = string.Empty;
}
