using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Infrastructure.Persistence.Repositories
{
    public class ProductoRepository: IProductoRepository
    {
        private readonly AppDbContext _context;

        public ProductoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Producto?> ObtenerPorIdAsync(int id)
        {
            return await _context.Productos
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IReadOnlyList<Producto>> ObtenerTodosAsync()
        {
            return await _context.Productos
                .AsNoTracking()
                .Include(p => p.Categoria)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }
        public async Task AgregarAsync(Producto producto)
        {
            await _context.Productos.AddAsync(producto);
        }


        public Task EliminarAsync(Producto producto)
        {
            _context.Productos.Remove(producto);

            return Task.CompletedTask;
        }

        public async Task<bool> ExisteCodigoInternoAsync(string codigoInterno)
        {
            return await _context.Productos
                .AnyAsync(p => p.CodigoInterno == codigoInterno);
        }

    }
}
