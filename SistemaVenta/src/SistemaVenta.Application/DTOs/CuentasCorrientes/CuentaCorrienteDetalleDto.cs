
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Application.DTOs.CuentasCorrientes;

public class CuentaCorrienteDetalleDto
{
    public int Id { get; set; }

    public int ClienteId { get; set; }

    public string Cliente { get; set; } = string.Empty;

    public string Documento { get; set; } = string.Empty;

    public decimal SaldoActual { get; set; }

    public decimal LimiteDeuda { get; set; }

    public decimal CreditoDisponible { get; set; }

    public DateTime FechaApertura { get; set; }

    public EstadoCuentaCorriente Estado { get; set; }

    public string? Observaciones { get; set; }

    public List<MovimientoCuentaCorrienteDto> Movimientos { get; set; }
        = new();
}
