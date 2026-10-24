
using SistemaVenta.Application.DTOs.Ventas;
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Application.UseCases.Clientes;

namespace SistemaVenta.Application.UseCases.Ventas
{
    public class ObtenerDetalleVenta
    {
        private readonly IVentaRepository _ventaRepository;
        private readonly ListarClientes _listarClientes;

        public ObtenerDetalleVenta(
            IVentaRepository ventaRepository,
            ListarClientes listarClientes)
        {
            _ventaRepository = ventaRepository;
            _listarClientes = listarClientes;
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

            var clientes = await _listarClientes.EjecutarAsync();

            var cliente = clientes.FirstOrDefault(
                c => c.Id == venta.ClienteId);

            return new VentaDetalleDto
            {
                Id = venta.Id,
                ClienteId = venta.ClienteId,

                NombreCliente = cliente?.Nombre
                    ?? $"Cliente #{venta.ClienteId}",

                Fecha = venta.Fecha,
                Estado = venta.Estado,
                Total = venta.Total,

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
