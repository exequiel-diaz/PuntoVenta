
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Domain.Entities
{
    public class Venta
    {
        public int Id { get; private set; }

        public int? ClienteId { get; private set; }

        public DateTime Fecha { get; private set; }

        public EstadoVenta Estado { get; private set; }

        private readonly List<DetalleVenta> _detalles = new();

        public IReadOnlyCollection<DetalleVenta> Detalles =>
            _detalles.AsReadOnly();

        public decimal Total =>
            _detalles.Sum(d => d.Subtotal);

        private Venta()
        {
        }

        public Venta(int? clienteId)
        {
            if (clienteId.HasValue && clienteId.Value <= 0)
            {
                throw new ArgumentException(
                    "El cliente debe ser válido.",
                    nameof(clienteId));
            }

            ClienteId = clienteId;
            Fecha = DateTime.UtcNow;
            Estado = EstadoVenta.Pendiente;
        }

        public void AgregarDetalle(
            Producto producto,
            decimal cantidad,
            decimal precioUnitario)
        {
            if (Estado != EstadoVenta.Pendiente)
            {
                throw new InvalidOperationException(
                    "Solo se pueden agregar productos a una venta pendiente.");
            }

            if (producto is null)
            {
                throw new ArgumentNullException(nameof(producto));
            }

            if (producto.Estado != EstadoProducto.Activo)
            {
                throw new InvalidOperationException(
                    "No se puede vender un producto inactivo.");
            }


            if (_detalles.Any(d =>
                ReferenceEquals(d.Producto, producto) ||
                (producto.Id > 0 && d.ProductoId == producto.Id)))
            {
                throw new InvalidOperationException(
                    "El producto ya fue agregado a la venta.");
            }


            var detalle = new DetalleVenta(
                producto,
                cantidad,
                precioUnitario);

            _detalles.Add(detalle);
        }

        public void Confirmar()
        {
            if (Estado != EstadoVenta.Pendiente)
            {
                throw new InvalidOperationException(
                    "Solo se puede confirmar una venta pendiente.");
            }

            if (!_detalles.Any())
            {
                throw new InvalidOperationException(
                    "No se puede confirmar una venta sin productos.");
            }

            Estado = EstadoVenta.Confirmada;
        }

        public void Anular()
        {
            if (Estado != EstadoVenta.Confirmada)
            {
                throw new InvalidOperationException(
                    "Solo se puede anular una venta confirmada.");
            }

            Estado = EstadoVenta.Anulada;
        }


    }
}
