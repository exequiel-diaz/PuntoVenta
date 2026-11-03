using SistemaVenta.Application.Interfaces;
using SistemaVenta.Application.UseCases.Pagos;
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Application.UseCases.CuentasCorrientes;

public class RegistrarCobroVentaCuentaCorriente
{
    private readonly RegistrarPago _registrarPago;
    private readonly AplicarPagoVentaCuentaCorriente _aplicarPago;
    private readonly ITransaccionCobroVenta _transaccion;

    public RegistrarCobroVentaCuentaCorriente(
        RegistrarPago registrarPago,
        AplicarPagoVentaCuentaCorriente aplicarPago,
        ITransaccionCobroVenta transaccion)
    {
        _registrarPago = registrarPago;
        _aplicarPago = aplicarPago;
        _transaccion = transaccion;
    }

    public async Task<int> EjecutarAsync(
        int ventaId,
        decimal importe,
        FormaPago formaPago,
        int usuarioId,
        string? observaciones = null,
        CancellationToken cancellationToken = default)
    {
        if (ventaId <= 0)
            throw new ArgumentException(
                "La venta debe ser válida.");

        if (importe <= 0)
            throw new ArgumentException(
                "El importe debe ser mayor que cero.");

        return await _transaccion.EjecutarAsync(
            async () =>
            {
                // Registrar el pago.
                var pagoId = await _registrarPago.EjecutarAsync(
                    importe,
                    formaPago,
                    usuarioId,
                    observaciones,
                    cancellationToken);

                // Aplicar el pago a la venta y actualizar la cuenta.
                await _aplicarPago.EjecutarAsync(
                    ventaId,
                    pagoId,
                    importe,
                    cancellationToken);

                // Devolver el ID para visualizar el recibo.
                return pagoId;
            },
            cancellationToken);
    }
}