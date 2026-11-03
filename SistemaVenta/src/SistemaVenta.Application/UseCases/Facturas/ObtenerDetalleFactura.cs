
using SistemaVenta.Application.DTOs.Facturas;
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Application.UseCases.Ventas;
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Application.UseCases.Facturas
{
    public class ObtenerDetalleFactura
    {
        private readonly IDocumentoComercialRepository _documentos;
        private readonly ObtenerDetalleVenta _obtenerDetalleVenta;

        public ObtenerDetalleFactura(
            IDocumentoComercialRepository documentos,
            ObtenerDetalleVenta obtenerDetalleVenta)
        {
            _documentos = documentos;
            _obtenerDetalleVenta = obtenerDetalleVenta;
        }

        public async Task<FacturaDetalleDto?> EjecutarAsync(int facturaId)
        {
            if (facturaId <= 0)
                return null;

            var documento = await _documentos.ObtenerPorIdAsync(facturaId);

            if (documento is not Factura factura)
                return null;

            var venta = await _obtenerDetalleVenta.EjecutarAsync(
                factura.VentaId);

            if (venta is null)
                return null;

            return new FacturaDetalleDto
            {
                Id = factura.Id,
                VentaId = factura.VentaId,
                Numero = factura.Numero,
                Tipo = factura.Tipo,
                FechaEmision = factura.FechaEmision,
                Estado = factura.Estado,
                NombreCliente = venta.NombreCliente,
                ImporteTotal = factura.ImporteTotal,
                Observaciones = factura.Observaciones,
                Detalles = venta.Detalles
            };
        }
    }
}
