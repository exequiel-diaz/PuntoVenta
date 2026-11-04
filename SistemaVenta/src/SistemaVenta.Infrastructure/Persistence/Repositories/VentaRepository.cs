
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


        public async Task<IReadOnlyList<Venta>> ObtenerPorPeriodoAsync(
            DateTime desde,
            DateTime hasta)
        {
            if (desde >= hasta)
            {
                throw new ArgumentException(
                    "La fecha inicial debe ser anterior a la fecha final.");
            }

            return await _context.Ventas
                .AsNoTracking()
                .Include(v => v.Detalles)
                .Where(v =>
                    v.Fecha >= desde &&
                    v.Fecha < hasta)
                .OrderByDescending(v => v.Fecha)
                .ToListAsync();
        }


        public async Task<bool> TieneVentasPorVendedorAsync(int vendedorId)
        {
            return await _context.Ventas
                .AsNoTracking()
                .AnyAsync(v => v.VendedorId == vendedorId);
        }


        public async Task<bool> TieneVentasPorProductoAsync(int productoId)
        {
            return await _context.Ventas
                .AsNoTracking()
                .AnyAsync(v => v.Detalles.Any(d => d.ProductoId == productoId));
        }


        public async Task AgregarAsync(Venta venta)
        {
            await _context.Ventas.AddAsync(venta);
        }
    }
}
