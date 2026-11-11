
using System.ComponentModel.DataAnnotations;

namespace SistemaVenta.Web.ViewModels.ConfiguracionCredito;

public class ConfiguracionCreditoViewModel
{
    [Required(ErrorMessage = "Ingresá un límite de crédito.")]
    [Range(
        typeof(decimal),
        "0.01",
        "999999999999.99",
        ErrorMessage = "El límite debe ser mayor que cero.")]
    [Display(Name = "Límite predeterminado de crédito")]
    public decimal LimitePredeterminado { get; set; }
}
