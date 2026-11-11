
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Constants;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Application.UseCases.Usuarios;

public class ActualizarPermisosUsuario
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IPermisoUsuarioRepository _permisos;
    private readonly IUnitOfWork _unitOfWork;

    public ActualizarPermisosUsuario(
        IUsuarioRepository usuarios,
        IPermisoUsuarioRepository permisos,
        IUnitOfWork unitOfWork)
    {
        _usuarios = usuarios;
        _permisos = permisos;
        _unitOfWork = unitOfWork;
    }

    public async Task EjecutarAsync(
        int usuarioId,
        IEnumerable<string> codigosPermisos)
    {
        ArgumentNullException.ThrowIfNull(codigosPermisos);

        var usuario = await _usuarios.ObtenerPorIdAsync(
            usuarioId);

        if (usuario is null)
            throw new InvalidOperationException(
                "El usuario no existe.");

        if (usuario.Rol != RolUsuario.Vendedor)
            throw new InvalidOperationException(
                "Solo se pueden asignar permisos a vendedores.");

        if (!usuario.Activo)
            throw new InvalidOperationException(
                "No se pueden asignar permisos a usuarios inactivos.");

        var nuevosPermisos = codigosPermisos
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (nuevosPermisos.Any(p =>
            !Permisos.Delegables.Contains(p)))
        {
            throw new ArgumentException(
                "Se intentó asignar un permiso no permitido.");
        }

        var actuales = await _permisos.ObtenerCodigosAsync(
            usuarioId);

        var actualesSet = actuales.ToHashSet(
            StringComparer.Ordinal);

        var nuevosSet = nuevosPermisos.ToHashSet(
            StringComparer.Ordinal);

        // Solo eliminamos los permisos revocados.
        // Así evitamos borrar y volver a insertar
        // los permisos que no cambiaron.
        var revocados = actualesSet
            .Except(nuevosSet)
            .ToHashSet(StringComparer.Ordinal);

        if (revocados.Count > 0)
        {
            // El repositorio actual permite eliminar
            // todos. Reemplazamos el conjunto completo
            // cuando hay revocaciones.
            await _permisos.EliminarTodosAsync(usuarioId);

            foreach (var codigo in nuevosPermisos)
            {
                await _permisos.AgregarAsync(
                    new PermisoUsuario(usuarioId, codigo));
            }
        }
        else
        {
            foreach (var codigo in nuevosSet.Except(actualesSet))
            {
                await _permisos.AgregarAsync(
                    new PermisoUsuario(usuarioId, codigo));
            }
        }

        await _unitOfWork.GuardarCambiosAsync();
    }
}
