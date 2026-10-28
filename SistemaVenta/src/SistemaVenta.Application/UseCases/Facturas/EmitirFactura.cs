
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Application.UseCases.Facturas
{
    public class EmitirFactura
    {
        private readonly IVentaRepository _ventas;
        private readonly IDocumentoComercialRepository _documentos;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITransaccionVenta _transaccion;

        public EmitirFactura(
            IVentaRepository ventas,
            IDocumentoComercialRepository documentos,
            IUnitOfWork unitOfWork,
            ITransaccionVenta transaccion)
        {
            _ventas = ventas;
            _documentos = documentos;
            _unitOfWork = unitOfWork;
            _transaccion = transaccion;
        }

        public async Task<int> EjecutarAsync(
            int ventaId,
            TipoFactura tipo)
        {
            if (ventaId <= 0)
                throw new ArgumentException(
                    "El identificador de la venta debe ser válido.");

            int facturaId = 0;

            await _transaccion.EjecutarAsync(
                ventaId,
                async () =>
                {
                    var venta = await _ventas.ObtenerPorIdAsync(ventaId);

                    if (venta is null)
                        throw new InvalidOperationException(
                            "La venta no existe.");

                    if (venta.Estado != EstadoVenta.Confirmada)
                        throw new InvalidOperationException(
                            "Solo se pueden facturar ventas confirmadas.");

                    if (venta.Total <= 0)
                        throw new InvalidOperationException(
                            "El importe de la venta debe ser mayor que cero.");

                    var facturaExistente =
                        await _documentos.ObtenerFacturaPorVentaIdAsync(
                            ventaId);

                    if (facturaExistente is not null)
                        throw new InvalidOperationException(
                            "La venta ya tiene una factura registrada.");

                    var numero = $"INT-{Guid.NewGuid():N}";

                    var factura = new Factura(
                        ventaId,
                        tipo,
                        venta.Total,
                        numero);

                    await _documentos.AgregarAsync(factura);

                    await _unitOfWork.GuardarCambiosAsync();

                    facturaId = factura.Id;
                });

            return facturaId;
        }
    }
}
