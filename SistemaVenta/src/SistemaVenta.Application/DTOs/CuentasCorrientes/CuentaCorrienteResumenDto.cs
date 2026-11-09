
namespace SistemaVenta.Application.DTOs.CuentasCorrientes;

public class CuentaCorrienteResumenDto
{
    public int Id { get; set; }

    public int ClienteId { get; set; }

    public string Cliente { get; set; } = string.Empty;

    public string Documento { get; set; } = string.Empty;

    public decimal SaldoActual { get; set; }

    public decimal LimiteDeuda { get; set; }

    public string Estado { get; set; } = string.Empty;
}
