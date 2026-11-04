
using Microsoft.EntityFrameworkCore;
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Infrastructure.Persistence.Repositories
{
    public class MovimientoStockRepository
        : IMovimientoStockRepository
    {
        private readonly AppDbContext _context;

        public MovimientoStockRepository(
            AppDbContext context)
        {
            _context = context;
        }

        public async Task AgregarAsync(
            MovimientoStock movimiento)
        {
            await _context.MovimientosStock
                .AddAsync(movimiento);
        }


        public async Task<bool> TieneMovimientosPorProductoAsync(
            int productoId)
        {
            return await _context.MovimientosStock
                .AsNoTracking()
                .AnyAsync(m => m.Producto.Id == productoId);
        }

    }
}
