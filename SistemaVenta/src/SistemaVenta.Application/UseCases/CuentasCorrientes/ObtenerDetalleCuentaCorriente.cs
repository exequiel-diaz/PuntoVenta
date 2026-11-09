
using SistemaVenta.Application.DTOs.CuentasCorrientes;
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Application.UseCases.CuentasCorrientes;

public class ObtenerDetalleCuentaCorriente
{
    private readonly ICuentaCorrienteRepository _cuentas;
    private readonly IClienteRepository _clientes;

    public ObtenerDetalleCuentaCorriente(
        ICuentaCorrienteRepository cuentas,
        IClienteRepository clientes)
    {
        _cuentas = cuentas;
        _clientes = clientes;
    }

    public async Task<CuentaCorrienteDetalleDto?> EjecutarAsync(
        int cuentaId)
    {
        var cuenta = await _cuentas.ObtenerPorIdAsync(cuentaId);

        if (cuenta is null)
            return null;

        var cliente = await _clientes.ObtenerPorIdAsync(
            cuenta.ClienteId);

        string nombre = "Cliente no encontrado";
        string documento = string.Empty;

        switch (cliente)
        {
            case ClienteFisico fisico:
                nombre = $"{fisico.Nombre} {fisico.Apellido}";
                documento = fisico.Dni;
                break;

            case ClienteEmpresa empresa:
                nombre = empresa.RazonSocial;
                documento = empresa.Cuit;
                break;
        }

        var movimientos = cuenta.Movimientos
            .OrderBy(m => m.Fecha)
            .ThenBy(m => m.Id)
            .ToList();

        var saldoAcumulado = 0m;
        var movimientosDto = new List<MovimientoCuentaCorrienteDto>();

        foreach (var movimiento in movimientos)
        {
            var debe = movimiento.EsDebito()
                ? movimiento.Importe
                : 0m;

            var haber = movimiento.EsCredito()
                ? movimiento.Importe
                : 0m;

            saldoAcumulado += debe - haber;

            movimientosDto.Add(new MovimientoCuentaCorrienteDto
            {
                Id = movimiento.Id,
                Fecha = movimiento.Fecha,
                Descripcion = movimiento.Descripcion,
                Tipo = movimiento.Tipo,
                Importe = movimiento.Importe,
                Origen = movimiento.Origen.ToString(),
                Debe = debe,
                Haber = haber,
                SaldoAcumulado = saldoAcumulado,
                VentaId = movimiento.VentaId,
                PagoId = movimiento.PagoId
            });
        }

        return new CuentaCorrienteDetalleDto
        {
            Id = cuenta.Id,
            ClienteId = cuenta.ClienteId,
            Cliente = nombre,
            Documento = documento,
            SaldoActual = cuenta.SaldoActual,
            LimiteDeuda = cuenta.LimiteDeuda,
            CreditoDisponible = Math.Max(
                0m,
                cuenta.LimiteDeuda - cuenta.SaldoActual),
            Estado = cuenta.Estado,
            FechaApertura = cuenta.FechaApertura,
            Observaciones = cuenta.Observaciones,
            Movimientos = movimientosDto
        };
    }
}
