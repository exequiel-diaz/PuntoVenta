
using System.ComponentModel.DataAnnotations;
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Web.Models.CuentasCorrientes;

public class RegistrarCobroViewModel
{
    public int CuentaId { get; set; }

    public string Cliente { get; set; } = string.Empty;

    [Range(1, int.MaxValue,
        ErrorMessage = "Debe seleccionar una venta.")]
    public int VentaId { get; set; }

    [Range(typeof(decimal), "0.01", "79228162514264337593543950335",
        ErrorMessage = "El importe debe ser mayor a cero.")]
    public decimal Importe { get; set; }

    [Required(ErrorMessage = "Seleccione una forma de pago.")]
    public FormaPago FormaPago { get; set; } = FormaPago.Efectivo;

    public string? Observaciones { get; set; }

    public List<VentaPendienteViewModel> VentasPendientes { get; set; }
        = new();
}

public class VentaPendienteViewModel
{
    public int VentaId { get; set; }

    public decimal SaldoPendiente { get; set; }
}
