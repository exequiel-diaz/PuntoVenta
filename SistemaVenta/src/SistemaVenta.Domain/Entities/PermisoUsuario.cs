
using SistemaVenta.Domain.Constants;

namespace SistemaVenta.Domain.Entities;

public class PermisoUsuario
{
    public int Id { get; private set; }

    public int UsuarioId { get; private set; }

    public string CodigoPermiso { get; private set; }
        = string.Empty;

    private PermisoUsuario() { }

    public PermisoUsuario(
        int usuarioId,
        string codigoPermiso)
    {
        if (usuarioId <= 0)
            throw new ArgumentException(
                "El usuario no es válido.");

        if (!Permisos.Delegables.Contains(codigoPermiso))
            throw new ArgumentException(
                "El permiso solicitado no es válido.");

        UsuarioId = usuarioId;
        CodigoPermiso = codigoPermiso;
    }
}
