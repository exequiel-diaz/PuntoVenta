
using Microsoft.EntityFrameworkCore;
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Infrastructure.Persistence.Repositories
{
    public class VentaRepository : IVentaRepository
    {
        private readonly AppDbContext _context;

        public VentaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Venta?> ObtenerPorIdAsync(int id)
        {
            return await _context.Ventas
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<IReadOnlyList<Venta>> ObtenerTodosAsync()
        {
            return await _context.Ventas
                .AsNoTracking()
                .Include(v => v.Detalles)
                .OrderByDescending(v => v.Fecha)
                .ToListAsync();
        }

        public async Task AgregarAsync(Venta venta)
        {
            await _context.Ventas.AddAsync(venta);
        }
    }
}
