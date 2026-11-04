
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


        public async Task<List<Usuario>> ObtenerVendedoresAsync()
        {
            return await _context.Usuarios
                .AsNoTracking()
                .Where(u => u.Rol == RolUsuario.Vendedor)
                .OrderBy(u => u.NombreCompleto)
                .ToListAsync();
        }


        public Task ActualizarAsync(Usuario usuario)
        {
            _context.Usuarios.Update(usuario);

            return Task.CompletedTask;
        }


        public Task EliminarAsync(Usuario usuario)
        {
            ArgumentNullException.ThrowIfNull(usuario);

            _context.Usuarios.Remove(usuario);

            return Task.CompletedTask;
        }


    }
}
