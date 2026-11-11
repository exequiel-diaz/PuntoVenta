
using Microsoft.EntityFrameworkCore;
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Application.DTOs.Dashboard;
using SistemaVenta.Domain.Enums;

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



        public async Task<IReadOnlyList<ProductoMasVendidoDto>>
            ObtenerProductosMasVendidosAsync(
                DateTime desde,
                DateTime hasta,
                int cantidad = 5)
        {
            if (desde >= hasta)
                throw new ArgumentException(
                    "El período de consulta no es válido.");

            if (cantidad <= 0)
                throw new ArgumentOutOfRangeException(nameof(cantidad));

            return await _context.Ventas
                .AsNoTracking()
                .Where(v =>
                    v.Fecha >= desde &&
                    v.Fecha < hasta &&
                    v.Estado == EstadoVenta.Confirmada)
                .SelectMany(v => v.Detalles)
                .GroupBy(d => new
                {
                    d.ProductoId,
                    d.Producto.Nombre
                })
                .Select(g => new ProductoMasVendidoDto
                {
                    ProductoId = g.Key.ProductoId,
                    Nombre = g.Key.Nombre,
                    CantidadVendida = g.Sum(d => d.Cantidad)
                })
                .OrderByDescending(p => p.CantidadVendida)
                .ThenBy(p => p.Nombre)
                .Take(cantidad)
                .ToListAsync();
        }


        public async Task AgregarAsync(Venta venta)
        {
            await _context.Ventas.AddAsync(venta);
        }

    }
}
