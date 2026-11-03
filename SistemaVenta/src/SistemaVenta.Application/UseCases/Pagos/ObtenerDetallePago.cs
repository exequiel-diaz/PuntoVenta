
using SistemaVenta.Application.DTOs.Pagos;
using SistemaVenta.Application.Interfaces;

namespace SistemaVenta.Application.UseCases.Pagos
{
    public class ObtenerDetallePago
    {
        private readonly IPagoRepository _pagos;

        public ObtenerDetallePago(IPagoRepository pagos)
        {
            _pagos = pagos;
        }

        public async Task<PagoDetalleDto?> EjecutarAsync(int pagoId)
        {
            if (pagoId <= 0)
                return null;

            var pago = await _pagos.ObtenerPorIdAsync(pagoId);

            if (pago is null)
                return null;

            var aplicaciones = pago.Aplicaciones
                .Select(a => new PagoAplicacionDto
                {
                    Tipo = "Factura / Documento",
                    ReferenciaId = a.DocumentoComercialId,
                    ImporteAplicado = a.ImporteAplicado
                })
                .Concat(
                    pago.AplicacionesVentas.Select(a => new PagoAplicacionDto
                    {
                        Tipo = "Venta",
                        ReferenciaId = a.VentaId,
                        ImporteAplicado = a.ImporteAplicado
                    }))
                .ToList();

            return new PagoDetalleDto
            {
                Id = pago.Id,
                Fecha = pago.Fecha,
                Importe = pago.Importe,
                FormaPago = pago.FormaPago,
                Estado = pago.Estado,
                Observaciones = pago.Observaciones,
                Aplicaciones = aplicaciones
            };
        }
    }
}
