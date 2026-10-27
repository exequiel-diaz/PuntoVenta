
using Microsoft.EntityFrameworkCore;
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Infrastructure.Persistence;

namespace SistemaVenta.Infrastructure.Repositories
{
    public class PagoRepository : IPagoRepository
    {
        private readonly AppDbContext _context;

        public PagoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Pago?> ObtenerPorIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await _context.Pagos
                .Include(p => p.Aplicaciones)
                .FirstOrDefaultAsync(
                    p => p.Id == id,
                    cancellationToken);
        }

        public async Task AgregarAsync(
            Pago pago,
            CancellationToken cancellationToken = default)
        {
            await _context.Pagos.AddAsync(
                pago,
                cancellationToken);
        }
    }
}
