
using Microsoft.EntityFrameworkCore;
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Infrastructure.Persistence;

namespace SistemaVenta.Infrastructure.Repositories;

public class PermisoUsuarioRepository
    : IPermisoUsuarioRepository
{
    private readonly AppDbContext _context;

    public PermisoUsuarioRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<string>> ObtenerCodigosAsync(
        int usuarioId)
    {
        return _context.PermisosUsuario
            .AsNoTracking()
            .Where(p => p.UsuarioId == usuarioId)
            .Select(p => p.CodigoPermiso)
            .ToListAsync();
    }

    public Task<bool> TienePermisoAsync(
        int usuarioId,
        string codigoPermiso)
    {
        return _context.PermisosUsuario
            .AsNoTracking()
            .AnyAsync(p =>
                p.UsuarioId == usuarioId &&
                p.CodigoPermiso == codigoPermiso);
    }

    public async Task AgregarAsync(PermisoUsuario permiso)
    {
        await _context.PermisosUsuario.AddAsync(permiso);
    }

    public async Task EliminarTodosAsync(int usuarioId)
    {
        var permisos = await _context.PermisosUsuario
            .Where(p => p.UsuarioId == usuarioId)
            .ToListAsync();

        _context.PermisosUsuario.RemoveRange(permisos);
    }
}
