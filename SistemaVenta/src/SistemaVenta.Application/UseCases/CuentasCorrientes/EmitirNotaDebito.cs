
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Application.UseCases.DocumentosComerciales;

public class EmitirNotaDebito
{
    private readonly IVentaRepository _ventas;
    private readonly IDocumentoComercialRepository _documentos;
    private readonly ICuentaCorrienteRepository _cuentas;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITransaccionVenta _transaccion;

    public EmitirNotaDebito(
        IVentaRepository ventas,
        IDocumentoComercialRepository documentos,
        ICuentaCorrienteRepository cuentas,
        IUnitOfWork unitOfWork,
        ITransaccionVenta transaccion)
    {
        _ventas = ventas;
        _documentos = documentos;
        _cuentas = cuentas;
        _unitOfWork = unitOfWork;
        _transaccion = transaccion;
    }

    public async Task<int> EjecutarAsync(
        int facturaId,
        string motivo,
        decimal importeTotal)
    {
        if (facturaId <= 0)
            throw new ArgumentException("La factura debe ser válida.");

        if (string.IsNullOrWhiteSpace(motivo))
            throw new ArgumentException("El motivo es obligatorio.");

        if (importeTotal <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(importeTotal),
                "El importe debe ser mayor que cero.");

        var documento = await _documentos.ObtenerPorIdAsync(facturaId);

        if (documento is not Factura factura)
            throw new InvalidOperationException("La factura no existe.");

        int notaDebitoId = 0;

        await _transaccion.EjecutarAsync(factura.VentaId, async () =>
        {
            var documentoActual =
                await _documentos.ObtenerPorIdAsync(facturaId);

            if (documentoActual is not Factura facturaActual)
                throw new InvalidOperationException("La factura no existe.");

            if (facturaActual.Estado != EstadoDocumento.Emitido)
                throw new InvalidOperationException(
                    "La factura no está emitida.");

            var venta = await _ventas.ObtenerPorIdAsync(
                facturaActual.VentaId);

            if (venta is null ||
                venta.Estado != EstadoVenta.Confirmada)
                throw new InvalidOperationException(
                    "La venta asociada no está confirmada.");


            var numero = $"INT-ND-{Guid.NewGuid():N}";

            var notaDebito = new NotaDebito(
                facturaId,
                numero,
                motivo,
                importeTotal);

            // Si la venta tiene Cuenta Corriente,
            // registramos el aumento de la deuda.
            if (venta.ClienteId.HasValue)
            {
                var cuenta = await _cuentas.ObtenerPorClienteIdAsync(
                    venta.ClienteId.Value);

                if (cuenta is not null && cuenta.TieneDebitoDeVenta(venta.Id))
                {
                    cuenta.RegistrarNotaDebito(notaDebito, venta);
                }
            }

            // Guardamos la nota y el movimiento.
            await _documentos.AgregarAsync(notaDebito);
            await _unitOfWork.GuardarCambiosAsync();

            notaDebitoId = notaDebito.Id;

        });

        return notaDebitoId;
    }
}
