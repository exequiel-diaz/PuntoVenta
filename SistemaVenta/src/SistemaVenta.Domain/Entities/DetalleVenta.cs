
namespace SistemaVenta.Domain.Entities
{
    public class DetalleVenta
    {
        public int Id { get; private set; }

        public int VentaId { get; private set; }

        public int ProductoId { get; private set; }

        public decimal Cantidad { get; private set; }

        public decimal PrecioUnitario { get; private set; }

        public decimal Subtotal => Cantidad * PrecioUnitario;

        public Producto Producto { get; private set; }

        private DetalleVenta()
        {
            Producto = null!;
        }

        public DetalleVenta(
            Producto producto,
            decimal cantidad,
            decimal precioUnitario)
        {
            if (producto is null)
            {
                throw new ArgumentNullException(
                    nameof(producto),
                    "El producto es obligatorio.");
            }

            if (cantidad <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(cantidad),
                    "La cantidad debe ser mayor que cero.");
            }

            if (precioUnitario < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(precioUnitario),
                    "El precio unitario no puede ser negativo.");
            }

            Producto = producto;
            ProductoId = producto.Id;
            Cantidad = cantidad;
            PrecioUnitario = precioUnitario;
        }
    }
}
