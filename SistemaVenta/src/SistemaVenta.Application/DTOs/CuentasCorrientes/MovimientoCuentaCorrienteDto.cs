
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Application.DTOs.CuentasCorrientes;

public class MovimientoCuentaCorrienteDto
{
    public int Id { get; set; }

    public DateTime Fecha { get; set; }

    public string Descripcion { get; set; } = string.Empty;

    public TipoMovimientoCuenta Tipo { get; set; }

    public string Origen { get; set; } = string.Empty;

    public decimal Importe { get; set; }

    public decimal Debe { get; set; }

    public decimal Haber { get; set; }

    public decimal SaldoAcumulado { get; set; }

    public int? VentaId { get; set; }

    public int? PagoId { get; set; }
}
