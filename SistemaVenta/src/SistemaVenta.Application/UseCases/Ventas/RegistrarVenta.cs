
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
        private readonly ICuentaCorrienteRepository _cuentas;
        private readonly IUsuarioRepository _usuarios;

        public RegistrarVenta(
            IVentaRepository ventas,
            IClienteRepository clientes,
            IProductoRepository productos,
            IMovimientoStockRepository movimientos,
            IUnitOfWork unitOfWork,
            ICuentaCorrienteRepository cuentas,
            IUsuarioRepository usuarios)
        {
            _ventas = ventas;
            _clientes = clientes;
            _productos = productos;
            _movimientos = movimientos;
            _unitOfWork = unitOfWork;
            _cuentas = cuentas;
            _usuarios = usuarios;
        }

        public async Task<int> EjecutarAsync(
            RegistrarVentaDto solicitud,
            int usuarioId)
        {

            if (usuarioId <= 0)
            {
                throw new ArgumentException(
                    "El usuario que registra la venta no es válido.");
            }

            var usuario = await _usuarios.ObtenerPorIdAsync(usuarioId);

            if (usuario is null || !usuario.Activo)
            {
                throw new InvalidOperationException(
                    "El usuario no existe o está desactivado.");
            }

            if (usuario.Rol != RolUsuario.Vendedor &&
                usuario.Rol != RolUsuario.Administrador)
            {
                throw new InvalidOperationException(
                    "El usuario no está autorizado para registrar ventas.");
            }


            if (solicitud is null)
            {
                throw new ArgumentNullException(nameof(solicitud));
            }

            if (solicitud.ClienteId.HasValue &&
                solicitud.ClienteId.Value <= 0)
            {
                throw new ArgumentException(
                    "El cliente debe ser válido.");
            }

            if (!Enum.IsDefined(solicitud.Modalidad))
            {
                throw new ArgumentException(
                    "La modalidad de venta no es válida.");
            }

            if (solicitud.Modalidad == ModalidadVenta.CuentaCorriente &&
                !solicitud.ClienteId.HasValue)
            {
                throw new InvalidOperationException(
                    "Las ventas a cuenta corriente requieren un cliente registrado.");
            }

            if (solicitud.Detalles is null ||
                solicitud.Detalles.Count == 0)
            {
                throw new ArgumentException(
                    "La venta debe contener al menos un producto.");
            }

            // 1. Validar cliente

            // 1. Validar cliente, si corresponde

            if (solicitud.ClienteId.HasValue)
            {
                var cliente = await _clientes.ObtenerPorIdAsync(
                    solicitud.ClienteId.Value);

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
                        $"No hay stock suficiente para {producto.Nombre}. " +
                        $"Stock actual: {producto.StockActual}. " +
                        $"Cantidad solicitada: {detalle.Cantidad}.");
                }

                productosValidados.Add((
                    producto,
                    detalle.Cantidad));
            }

            // 4. Construir la venta

            var venta = new Venta(solicitud.ClienteId);
            venta.AsignarVendedor(usuario.Id);

            foreach (var item in productosValidados)
            {
                venta.AgregarDetalle(
                    item.Producto,
                    item.Cantidad,
                    item.Producto.PrecioVenta);
            }


            // 4.1. Validar modalidad de venta

            if (solicitud.Modalidad == ModalidadVenta.CuentaCorriente)
            {
                var cuenta = await _cuentas.ObtenerPorClienteIdAsync(
                    solicitud.ClienteId.Value);

                if (cuenta is null)
                {
                    throw new InvalidOperationException(
                        "El cliente no tiene una cuenta corriente habilitada.");
                }

                if (!cuenta.PuedeRegistrarDeuda(venta.Total))
                {
                    throw new InvalidOperationException(
                        "La cuenta corriente no está activa o no tiene crédito suficiente.");
                }
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


            // 7.1. Registrar deuda si la venta es a cuenta corriente

            if (solicitud.Modalidad == ModalidadVenta.CuentaCorriente)
            {
                var cuenta = await _cuentas.ObtenerPorClienteIdAsync(
                    solicitud.ClienteId.Value);

                if (cuenta is null)
                {
                    throw new InvalidOperationException(
                        "El cliente no tiene cuenta corriente.");
                }


                cuenta.RegistrarDebito(
                    venta.Total,
                    "Venta a cuenta corriente",
                    venta);

            }


            // 8. Persistir cambios

            await _unitOfWork.GuardarCambiosAsync();

            // 9. Devuelvo el identificador de la venta registraddas
            return venta.Id;
        }
    }
}
