
using SistemaVenta.Application.DTOs.Ventas;
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Application.UseCases.Clientes;

namespace SistemaVenta.Application.UseCases.Ventas
{
    public class ObtenerDetalleVenta
    {
        private readonly IVentaRepository _ventaRepository;
        private readonly ListarClientes _listarClientes;
        private readonly IDocumentoComercialRepository _documentos;

        public ObtenerDetalleVenta(
            IVentaRepository ventaRepository,
            ListarClientes listarClientes,
            IDocumentoComercialRepository documentos)
        {
            _ventaRepository = ventaRepository;
            _listarClientes = listarClientes;
            _documentos = documentos;
        }

        public async Task<VentaDetalleDto?> EjecutarAsync(int ventaId)
        {
            if (ventaId <= 0)
            {
                return null;
            }

            var venta = await _ventaRepository.ObtenerPorIdAsync(ventaId);

            if (venta is null)
            {
                return null;
            }

            var factura = await _documentos.ObtenerFacturaPorVentaIdAsync(ventaId);

            var clientes = await _listarClientes.EjecutarAsync();

            var cliente = venta.ClienteId.HasValue
                ? clientes.FirstOrDefault(c => c.Id == venta.ClienteId.Value)
                : null;

            return new VentaDetalleDto
            {
                Id = venta.Id,
                ClienteId = venta.ClienteId,

                NombreCliente = !venta.ClienteId.HasValue
                    ? "Consumidor final"
                    : cliente?.Nombre ?? $"Cliente #{venta.ClienteId.Value}",

                Fecha = venta.Fecha,
                Estado = venta.Estado,
                Total = venta.Total,
                TieneFactura = factura is not null,

                Detalles = venta.Detalles
                    .Select(d => new DetalleVentaListadoDto
                    {
                        ProductoId = d.ProductoId,
                        NombreProducto = d.Producto.Nombre,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario,
                        Subtotal = d.Subtotal
                    })
                    .ToList()
            };
        }
    }
}
