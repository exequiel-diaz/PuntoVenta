
using Microsoft.EntityFrameworkCore;
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Infrastructure.Persistence;
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Infrastructure.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly AppDbContext _context;

        public UsuarioRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Usuario?> ObtenerPorIdAsync(int id)
        {
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<Usuario?> ObtenerPorNombreUsuarioAsync(
            string nombreUsuario)
        {
            return await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.NombreUsuario.ToLower() ==
                    nombreUsuario.ToLower());
        }

        public async Task AgregarAsync(Usuario usuario)
        {
            await _context.Usuarios.AddAsync(usuario);
        }

        public async Task<bool> ExisteAdministradorAsync()
        {
            return await _context.Usuarios
                .AnyAsync(u => u.Rol == RolUsuario.Administrador);
        }
    }
}
