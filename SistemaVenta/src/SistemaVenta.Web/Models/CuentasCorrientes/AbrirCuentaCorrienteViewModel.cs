
using System.ComponentModel.DataAnnotations;

namespace SistemaVenta.Web.ViewModels.CuentasCorrientes;

public class AbrirCuentaCorrienteViewModel
{
    [Range(1, int.MaxValue,
        ErrorMessage = "Debe seleccionar un cliente válido.")]
    public int ClienteId { get; set; }

    [Range(0, double.MaxValue,
        ErrorMessage = "El límite de deuda no puede ser negativo.")]
    [Display(Name = "Límite de deuda")]
    public decimal LimiteDeuda { get; set; }

    [StringLength(500,
        ErrorMessage = "Las observaciones no pueden superar los 500 caracteres.")]
    public string? Observaciones { get; set; }
}
