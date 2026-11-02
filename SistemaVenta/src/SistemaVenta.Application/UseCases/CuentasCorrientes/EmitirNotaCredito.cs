using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Application.UseCases.DocumentosComerciales
{
    public class EmitirNotaCredito
    {
        private readonly IVentaRepository _ventas;
        private readonly IDocumentoComercialRepository _documentos;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITransaccionVenta _transaccion;
        private readonly ICuentaCorrienteRepository _cuentas;

        public EmitirNotaCredito(
            IVentaRepository ventas,
            IDocumentoComercialRepository documentos,
            IUnitOfWork unitOfWork,
            ITransaccionVenta transaccion,
            ICuentaCorrienteRepository cuentas)
        {
            _ventas = ventas;
            _documentos = documentos;
            _unitOfWork = unitOfWork;
            _transaccion = transaccion;
            _cuentas = cuentas;
        }

        public async Task<int> EjecutarAsync(
            int facturaId,
            string motivo,
            decimal importeTotal)
        {
            if (facturaId <= 0)
                throw new ArgumentException(
                    "La factura debe ser válida.");

            if (string.IsNullOrWhiteSpace(motivo))
                throw new ArgumentException(
                    "El motivo es obligatorio.");

            if (importeTotal <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(importeTotal),
                    "El importe debe ser mayor que cero.");

            var documento = await _documentos.ObtenerPorIdAsync(
                facturaId);

            if (documento is not Factura factura)
                throw new InvalidOperationException(
                    "La factura no existe.");

            int notaCreditoId = 0;

            await _transaccion.EjecutarAsync(
                factura.VentaId,
                async () =>
                {
                    // Recuperar nuevamente la factura dentro
                    // de la operación protegida.
                    var documentoActual =
                        await _documentos.ObtenerPorIdAsync(
                            facturaId);

                    if (documentoActual is not Factura facturaActual)
                        throw new InvalidOperationException(
                            "La factura no existe.");

                    if (facturaActual.Estado != EstadoDocumento.Emitido)
                        throw new InvalidOperationException(
                            "La factura no está emitida.");

                    var venta = await _ventas.ObtenerPorIdAsync(
                        facturaActual.VentaId);

                    if (venta is null ||
                        venta.Estado != EstadoVenta.Confirmada)
                    {
                        throw new InvalidOperationException(
                            "La venta asociada no está confirmada.");
                    }

                    var totalCreditos =
                        await _documentos.ObtenerTotalNotasCreditoAsync(
                            facturaId);

                    var totalDebitos =
                        await _documentos.ObtenerTotalNotasDebitoAsync(
                            facturaId);

                    var importeDisponible =
                        facturaActual.ImporteTotal
                        + totalDebitos
                        - totalCreditos;

                    if (importeTotal > importeDisponible)
                        throw new InvalidOperationException(
                            "El importe de la Nota de Crédito supera " +
                            "el importe disponible de la factura.");


                    var numero = $"INT-NC-{Guid.NewGuid():N}";

                    var notaCredito = new NotaCredito(
                        facturaId,
                        numero,
                        motivo,
                        importeTotal);

                    if (venta.ClienteId.HasValue)
                    {
                        var cuenta = await _cuentas.ObtenerPorClienteIdAsync(
                            venta.ClienteId.Value);

                        if (cuenta is not null && cuenta.TieneDebitoDeVenta(venta.Id))
                        {
                            cuenta.RegistrarNotaCredito(notaCredito, venta);
                        }
                    }

                    // Guardamos el documento y el movimiento juntos.
                    await _documentos.AgregarAsync(notaCredito);
                    await _unitOfWork.GuardarCambiosAsync();

                    notaCreditoId = notaCredito.Id;

                });

            return notaCreditoId;
        }
    }
}