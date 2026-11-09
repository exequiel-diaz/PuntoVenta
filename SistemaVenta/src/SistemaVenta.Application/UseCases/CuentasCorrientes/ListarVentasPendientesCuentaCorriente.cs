
using SistemaVenta.Application.DTOs.CuentasCorrientes;
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Application.UseCases.CuentasCorrientes;

public class ListarVentasPendientesCuentaCorriente
{
    private readonly ICuentaCorrienteRepository _cuentaRepository;

    public ListarVentasPendientesCuentaCorriente(
        ICuentaCorrienteRepository cuentaRepository)
    {
        _cuentaRepository = cuentaRepository;
    }

    public async Task<IReadOnlyList<VentaPendienteCuentaCorrienteDto>>
        EjecutarAsync(int cuentaId)
    {
        if (cuentaId <= 0)
        {
            throw new ArgumentException(
                "El identificador de la cuenta debe ser válido.");
        }

        var cuenta = await _cuentaRepository.ObtenerPorIdAsync(cuentaId);

        if (cuenta is null)
        {
            throw new InvalidOperationException(
                "La cuenta corriente no existe.");
        }

        var ventaIds = cuenta.Movimientos
            .Where(m =>
                m.Tipo == TipoMovimientoCuenta.Debito &&
                m.VentaId.HasValue)
            .Select(m => m.VentaId!.Value)
            .Distinct()
            .ToList();

        var resultado = new List<VentaPendienteCuentaCorrienteDto>();

        foreach (var ventaId in ventaIds)
        {
            var saldo = cuenta.ObtenerSaldoPendienteDeVenta(ventaId);

            if (saldo > 0)
            {
                resultado.Add(new VentaPendienteCuentaCorrienteDto
                {
                    VentaId = ventaId,
                    SaldoPendiente = saldo
                });
            }
        }

        return resultado
            .OrderBy(v => v.VentaId)
            .ToList();
    }
}
