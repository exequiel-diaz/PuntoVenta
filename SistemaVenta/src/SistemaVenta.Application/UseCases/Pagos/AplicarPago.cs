
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Application.UseCases.Pagos
{
    public class AplicarPago
    {
        private readonly IPagoRepository _pagoRepository;
        private readonly IDocumentoComercialRepository _documentoRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITransaccionAplicacionPago _transaccion;

        public AplicarPago(
            IPagoRepository pagoRepository,
            IDocumentoComercialRepository documentoRepository,
            IUnitOfWork unitOfWork,
            ITransaccionAplicacionPago transaccion)
        {
            _pagoRepository = pagoRepository;
            _documentoRepository = documentoRepository;
            _unitOfWork = unitOfWork;
            _transaccion = transaccion;
        }

        public async Task EjecutarAsync(
            int pagoId,
            int documentoComercialId,
            decimal importeAplicado,
            CancellationToken cancellationToken = default)
        {
            if (importeAplicado <= 0)
                throw new ArgumentException(
                    "El importe aplicado debe ser mayor que cero.");

            await _transaccion.EjecutarAsync(
                documentoComercialId,
                pagoId,
                async () =>
                {
                    var pago =
                        await _pagoRepository.ObtenerPorIdAsync(
                            pagoId, cancellationToken);

                    if (pago is null)
                        throw new InvalidOperationException(
                            "El pago no existe.");

                    if (pago.Estado == EstadoPago.Anulado)
                        throw new InvalidOperationException(
                            "No se puede aplicar un pago anulado.");

                    var documento =
                        await _documentoRepository.ObtenerPorIdAsync(
                            documentoComercialId, cancellationToken);

                    if (documento is not Factura factura)
                        throw new InvalidOperationException(
                            "El documento no existe o no es una factura.");

                    if (factura.Estado != EstadoDocumento.Emitido)
                        throw new InvalidOperationException(
                            "La factura debe estar emitida.");

                    var totalAplicado =
                        await _documentoRepository.ObtenerTotalAplicadoAsync(
                            documentoComercialId, cancellationToken);

                    var saldoPendiente =
                        factura.ImporteTotal - totalAplicado;

                    if (importeAplicado > saldoPendiente)
                        throw new InvalidOperationException(
                            "El importe supera el saldo pendiente de la factura.");

                    pago.AgregarAplicacion(
                        documentoComercialId,
                        importeAplicado);

                    await _unitOfWork.GuardarCambiosAsync();
                },
                cancellationToken);
        }
    }
}
