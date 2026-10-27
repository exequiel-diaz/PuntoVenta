
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Application.Interfaces
{
    public interface IPagoRepository
    {
        Task<Pago?> ObtenerPorIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task AgregarAsync(
            Pago pago,
            CancellationToken cancellationToken = default);
    }
}
