
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Application.Tests.Fakes
{
    public class FakeMovimientoStockRepository
        : IMovimientoStockRepository
    {
        private readonly List<MovimientoStock> _movimientos = new();

        public IReadOnlyList<MovimientoStock> Movimientos =>
            _movimientos.AsReadOnly();

        public MovimientoStock? MovimientoAgregado =>
            _movimientos.LastOrDefault();

        public Task AgregarAsync(MovimientoStock movimiento)
        {
            _movimientos.Add(movimiento);

            return Task.CompletedTask;
        }

        public Task<bool> TieneMovimientosPorProductoAsync(int productoId)
        {
            bool tieneMovimientos = _movimientos
                .Any(m => m.Producto.Id == productoId);

            return Task.FromResult(tieneMovimientos);
        }
    }
}
