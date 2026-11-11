
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Application.Interfaces;

public interface IPermisoUsuarioRepository
{
    Task<List<string>> ObtenerCodigosAsync(int usuarioId);

    Task<bool> TienePermisoAsync(
        int usuarioId,
        string codigoPermiso);

    Task AgregarAsync(PermisoUsuario permiso);

    Task EliminarTodosAsync(int usuarioId);
}
