using System.ComponentModel.DataAnnotations;
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Web.ViewModels.Pagos;

public class RegistrarCobroVentaViewModel
{
    [Range(1, int.MaxValue,
        ErrorMessage = "La venta debe ser válida.")]
    public int VentaId { get; set; }
    public decimal SaldoPendiente { get; set; }

    [Required(ErrorMessage = "El importe es obligatorio.")]
    [Range(
    typeof(decimal),
    "0.01",
    "999999999999",
    ParseLimitsInInvariantCulture = true,
    ErrorMessage = "El importe debe ser mayor que cero.")]
    public decimal Importe { get; set; }

    [Required(ErrorMessage = "La forma de pago es obligatoria.")]
    public FormaPago? FormaPago { get; set; }

    [StringLength(500,
        ErrorMessage = "La observación no puede superar los 500 caracteres.")]
    public string? Observaciones { get; set; }
}