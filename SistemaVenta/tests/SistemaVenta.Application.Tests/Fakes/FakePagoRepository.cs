
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Application.Tests.Fakes
{
    public class FakePagoRepository : IPagoRepository
    {
        public List<Pago> Pagos { get; } = new();

        public Task AgregarAsync(
            Pago pago,
            CancellationToken cancellationToken = default)
        {
            Pagos.Add(pago);
            return Task.CompletedTask;
        }

        public Task<Pago?> ObtenerPorIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                Pagos.FirstOrDefault(p => p.Id == id));
        }
    }
}
