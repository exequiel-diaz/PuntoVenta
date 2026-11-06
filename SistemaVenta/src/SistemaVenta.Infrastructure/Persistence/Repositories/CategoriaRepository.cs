
using Microsoft.EntityFrameworkCore;
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Infrastructure.Persistence.Repositories
{
    public class CategoriaRepository : ICategoriaRepository
    {
        private readonly AppDbContext _context;

        public CategoriaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<Categoria>> ObtenerTodasAsync()
        {
            return await _context.Categorias
                .AsNoTracking()
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }

        public async Task<Categoria?> ObtenerPorIdAsync(int id)
        {
            return await _context.Categorias
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AgregarAsync(Categoria categoria)
        {
            await _context.Categorias.AddAsync(categoria);
        }

        public Task EliminarAsync(Categoria categoria)
        {
            _context.Categorias.Remove(categoria);

            return Task.CompletedTask;
        }

        public async Task<bool> TieneProductosAsync(int categoriaId)
        {
            return await _context.Productos
                .AnyAsync(p => p.Categoria.Id == categoriaId);
        }
    }
}
