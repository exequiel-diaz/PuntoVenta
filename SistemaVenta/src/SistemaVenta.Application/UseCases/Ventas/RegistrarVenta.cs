
using SistemaVenta.Application.DTOs.Ventas;
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Application.UseCases.Ventas
{
    public class RegistrarVenta
    {
        private readonly IVentaRepository _ventas;
        private readonly IClienteRepository _clientes;
        private readonly IProductoRepository _productos;
        private readonly IMovimientoStockRepository _movimientos;
        private readonly IUnitOfWork _unitOfWork;

        public RegistrarVenta(
            IVentaRepository ventas,
            IClienteRepository clientes,
            IProductoRepository productos,
            IMovimientoStockRepository movimientos,
            IUnitOfWork unitOfWork)
        {
            _ventas = ventas;
            _clientes = clientes;
            _productos = productos;
            _movimientos = movimientos;
            _unitOfWork = unitOfWork;
        }

        public async Task EjecutarAsync(RegistrarVentaDto solicitud)
        {
            if (solicitud is null)
            {
                throw new ArgumentNullException(nameof(solicitud));
            }

            if (solicitud.ClienteId <= 0)
            {
                throw new ArgumentException(
                    "El cliente debe ser válido.");
            }

            if (solicitud.Detalles is null ||
                solicitud.Detalles.Count == 0)
            {
                throw new ArgumentException(
                    "La venta debe contener al menos un producto.");
            }

            // 1. Validar cliente

            var cliente = await _clientes.ObtenerPorIdAsync(
                solicitud.ClienteId);

            if (cliente is null)
            {
                throw new InvalidOperationException(
                    "El cliente no existe.");
            }

            if (!cliente.EstaActivo())
            {
                throw new InvalidOperationException(
                    "No se puede vender a un cliente inactivo.");
            }

            // 2. Validar datos de entrada

            if (solicitud.Detalles.Any(d =>
                d.ProductoId <= 0 || d.Cantidad <= 0))
            {
                throw new ArgumentException(
                    "Los productos y cantidades deben ser válidos.");
            }

            if (solicitud.Detalles
                .GroupBy(d => d.ProductoId)
                .Any(g => g.Count() > 1))
            {
                throw new ArgumentException(
                    "No se permiten productos repetidos en una venta.");
            }

            // 3. Recuperar y validar todos los productos

            var productosValidados =
                new List<(Producto Producto, decimal Cantidad)>();

            foreach (var detalle in solicitud.Detalles)
            {
                var producto = await _productos.ObtenerPorIdAsync(
                    detalle.ProductoId);

                if (producto is null)
                {
                    throw new InvalidOperationException(
                        $"El producto {detalle.ProductoId} no existe.");
                }

                if (producto.Estado != EstadoProducto.Activo)
                {
                    throw new InvalidOperationException(
                        $"El producto {producto.Nombre} está inactivo.");
                }

                if (!producto.HayStockDisponible(detalle.Cantidad))
                {
                    throw new InvalidOperationException(
                        $"No hay stock suficiente para {producto.Nombre}.");
                }

                productosValidados.Add((
                    producto,
                    detalle.Cantidad));
            }

            // 4. Construir la venta

            var venta = new Venta(solicitud.ClienteId);

            foreach (var item in productosValidados)
            {
                venta.AgregarDetalle(
                    item.Producto,
                    item.Cantidad,
                    item.Producto.PrecioVenta);
            }

            // 5. Registrar salidas de inventario

            foreach (var item in productosValidados)
            {
                var stockAnterior = item.Producto.StockActual;

                item.Producto.DisminuirStock(item.Cantidad);

                var stockPosterior = item.Producto.StockActual;

                var movimiento = new MovimientoStock(
                    producto: item.Producto,
                    tipo: TipoMovimientoStock.Venta,
                    cantidad: item.Cantidad,
                    stockAnterior: stockAnterior,
                    stockPosterior: stockPosterior,
                    motivo: "Venta");

                await _movimientos.AgregarAsync(movimiento);
            }

            // 6. Confirmar venta

            venta.Confirmar();

            // 7. Registrar venta

            await _ventas.AgregarAsync(venta);

            // 8. Persistir cambios

            await _unitOfWork.GuardarCambiosAsync();
        }
    }
}
