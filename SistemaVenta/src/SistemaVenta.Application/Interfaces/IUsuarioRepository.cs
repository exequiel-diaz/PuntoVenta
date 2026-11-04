
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Application.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> ObtenerPorIdAsync(int id);

    Task<Usuario?> ObtenerPorNombreUsuarioAsync(
        string nombreUsuario);

    Task AgregarAsync(Usuario usuario);
    Task<bool> ExisteAdministradorAsync();
    Task<List<Usuario>> ObtenerVendedoresAsync();
    Task EliminarAsync(Usuario usuario);
    Task ActualizarAsync(Usuario usuario);


}
