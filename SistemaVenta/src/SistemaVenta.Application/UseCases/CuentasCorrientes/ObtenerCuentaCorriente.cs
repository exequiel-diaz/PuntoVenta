
using SistemaVenta.Application.DTOs.CuentasCorrientes;
using SistemaVenta.Application.Interfaces;

namespace SistemaVenta.Application.UseCases.CuentasCorrientes;

public class ObtenerCuentaCorriente
{
    private readonly ICuentaCorrienteRepository _cuentas;

    public ObtenerCuentaCorriente(
        ICuentaCorrienteRepository cuentas)
    {
        _cuentas = cuentas;
    }

    public async Task<CuentaCorrienteDetalleDto> EjecutarAsync(
        int clienteId)
    {
        var cuenta =
            await _cuentas.ObtenerPorClienteIdAsync(clienteId);

        if (cuenta is null)
            throw new InvalidOperationException(
                "El cliente no tiene cuenta corriente.");

        return new CuentaCorrienteDetalleDto
        {
            Id = cuenta.Id,
            ClienteId = cuenta.ClienteId,
            LimiteDeuda = cuenta.LimiteDeuda,
            SaldoActual = cuenta.SaldoActual,
            FechaApertura = cuenta.FechaApertura,
            Estado = cuenta.Estado,
            Observaciones = cuenta.Observaciones,

            Movimientos = cuenta.Movimientos
                .OrderByDescending(m => m.Fecha)
                .Select(m => new MovimientoCuentaCorrienteDto
                {
                    Id = m.Id,
                    Fecha = m.Fecha,
                    Tipo = m.Tipo,
                    Importe = m.Importe,
                    Descripcion = m.Descripcion
                })
                .ToList()
        };
    }
}
