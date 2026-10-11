
using Microsoft.EntityFrameworkCore;
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Infrastructure.Persistence;

namespace SistemaVenta.Infrastructure.Repositories
{
    public class ClienteRepository : IClienteRepository
    {
        private readonly AppDbContext _context;

        public ClienteRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Cliente?> ObtenerPorIdAsync(int id)
        {
            return await _context.Clientes
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IReadOnlyList<Cliente>> ObtenerTodosAsync()
        {
            return await _context.Clientes
                .AsNoTracking()
                .OrderBy(c => c.Id)
                .ToListAsync();
        }

        public async Task<bool> ExisteDniAsync(
            string dni,
            int? excluirId = null)
        {
            return await _context.Clientes
                .OfType<ClienteFisico>()
                .AnyAsync(c =>
                    c.Dni == dni &&
                    (!excluirId.HasValue || c.Id != excluirId.Value));
        }


        public async Task<bool> ExisteCuitAsync(
            string cuit,
            int? excluirId = null)
        {
            bool existeEnFisicos = await _context.Clientes
                .OfType<ClienteFisico>()
                .AnyAsync(c =>
                    c.Cuit == cuit &&
                    (!excluirId.HasValue || c.Id != excluirId.Value));

            if (existeEnFisicos)
                return true;

            return await _context.Clientes
                .OfType<ClienteEmpresa>()
                .AnyAsync(c =>
                    c.Cuit == cuit &&
                    (!excluirId.HasValue || c.Id != excluirId.Value));
        }


        public async Task<bool> ExisteEmailAsync(
            string email,
            int? excluirId = null)
        {
            return await _context.Clientes
                .AnyAsync(c =>
                    (!excluirId.HasValue || c.Id != excluirId.Value) &&
                    c.Email.ToLower() == email.ToLower());
        }

        public async Task AgregarAsync(Cliente cliente)
        {
            await _context.Clientes.AddAsync(cliente);
        }
    }
}
