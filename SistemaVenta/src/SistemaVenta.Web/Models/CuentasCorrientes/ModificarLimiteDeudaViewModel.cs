
using System.ComponentModel.DataAnnotations;

namespace SistemaVenta.Web.ViewModels.CuentasCorrientes;

public class ModificarLimiteDeudaViewModel
{
    [Range(1, int.MaxValue)]
    public int CuentaId { get; set; }

    [Range(1, int.MaxValue)]
    public int ClienteId { get; set; }

    [Display(Name = "Nuevo límite de deuda")]
    [Range(0, double.MaxValue,
        ErrorMessage = "El límite no puede ser negativo.")]
    public decimal NuevoLimite { get; set; }
}
