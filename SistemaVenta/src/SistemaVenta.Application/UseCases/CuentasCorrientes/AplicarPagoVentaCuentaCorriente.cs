using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Application.UseCases.CuentasCorrientes;

public class AplicarPagoVentaCuentaCorriente
{
    private readonly IPagoRepository _pagoRepository;
    private readonly IVentaRepository _ventaRepository;
    private readonly ICuentaCorrienteRepository _cuentaRepository;
    private readonly ITransaccionPagoVenta _transaccion;
    private readonly IUnitOfWork _unitOfWork;

    public AplicarPagoVentaCuentaCorriente(
        IPagoRepository pagoRepository,
        IVentaRepository ventaRepository,
        ICuentaCorrienteRepository cuentaRepository,
        ITransaccionPagoVenta transaccion,
        IUnitOfWork unitOfWork)
    {
        _pagoRepository = pagoRepository;
        _ventaRepository = ventaRepository;
        _cuentaRepository = cuentaRepository;
        _transaccion = transaccion;
        _unitOfWork = unitOfWork;
    }

    public async Task EjecutarAsync(
        int ventaId,
        int pagoId,
        decimal importe,
        CancellationToken cancellationToken = default)
    {
        if (ventaId <= 0)
            throw new ArgumentException(
                "El identificador de la venta debe ser válido.");

        if (pagoId <= 0)
            throw new ArgumentException(
                "El identificador del pago debe ser válido.");

        if (importe <= 0)
            throw new ArgumentException(
                "El importe debe ser mayor que cero.");

        await _transaccion.EjecutarAsync(
            ventaId,
            pagoId,
            async () =>
            {
                // Recuperar después de adquirir los bloqueos.
                var pago = await _pagoRepository.ObtenerPorIdAsync(
                    pagoId,
                    cancellationToken);

                if (pago is null)
                    throw new InvalidOperationException(
                        "El pago no existe.");

                var venta = await _ventaRepository.ObtenerPorIdAsync(
                    ventaId);

                if (venta is null)
                    throw new InvalidOperationException(
                        "La venta no existe.");

                if (venta.Estado != EstadoVenta.Confirmada)
                    throw new InvalidOperationException(
                        "Solo se pueden aplicar pagos a ventas confirmadas.");

                if (!venta.ClienteId.HasValue)
                {
                    throw new InvalidOperationException(
                        "No se pueden aplicar pagos de cuenta corriente " +
                        "a una venta sin cliente registrado.");
                }

                var cuenta =
                    await _cuentaRepository.ObtenerPorClienteIdAsync(
                        venta.ClienteId.Value);

                if (cuenta is null)
                {
                    throw new InvalidOperationException(
                        "El cliente no tiene una cuenta corriente.");
                }

                // Validar la deuda antes de modificar el pago.
                if (importe >
                    cuenta.ObtenerSaldoPendienteDeVenta(venta.Id))
                {
                    throw new InvalidOperationException(
                        "El importe supera el saldo pendiente de la venta.");
                }

                // Registrar ambas operaciones en memoria.
                pago.AgregarAplicacionVenta(venta, importe);

                cuenta.RegistrarPagoDeVenta(
                    venta,
                    pago,
                    importe);

                // Persistir ambos cambios en la misma transacción.
                await _unitOfWork.GuardarCambiosAsync();
            },
            cancellationToken);
    }
}