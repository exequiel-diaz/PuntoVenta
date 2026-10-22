
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Application.DTOs.CuentasCorrientes;

public class MovimientoCuentaCorrienteDto
{
    public int Id { get; set; }

    public DateTime Fecha { get; set; }

    public TipoMovimientoCuenta Tipo { get; set; }

    public decimal Importe { get; set; }

    public string Descripcion { get; set; } = string.Empty;
}
