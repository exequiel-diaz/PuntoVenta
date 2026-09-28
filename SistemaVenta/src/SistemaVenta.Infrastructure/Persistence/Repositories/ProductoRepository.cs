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
    }
}
