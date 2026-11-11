
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Application.UseCases.Usuarios;

public class ObtenerPermisosUsuario
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IPermisoUsuarioRepository _permisos;

    public ObtenerPermisosUsuario(
        IUsuarioRepository usuarios,
        IPermisoUsuarioRepository permisos)
    {
        _usuarios = usuarios;
        _permisos = permisos;
    }

    public async Task<List<string>> EjecutarAsync(
        int usuarioId)
    {
        var usuario = await _usuarios.ObtenerPorIdAsync(
            usuarioId);

        if (usuario is null)
            throw new InvalidOperationException(
                "El usuario no existe.");

        if (usuario.Rol != RolUsuario.Vendedor)
            throw new InvalidOperationException(
                "Solo se administran permisos individuales de vendedores.");

        return await _permisos.ObtenerCodigosAsync(usuarioId);
    }
}
