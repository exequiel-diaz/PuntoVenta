
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

        // Conservamos esta propiedad para no romper
        // las pruebas existentes.
        public MovimientoStock? MovimientoAgregado =>
            _movimientos.LastOrDefault();

        public Task AgregarAsync(MovimientoStock movimiento)
        {
            _movimientos.Add(movimiento);

            return Task.CompletedTask;
        }
    }
}
