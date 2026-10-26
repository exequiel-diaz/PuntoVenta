
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Application.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> ObtenerPorIdAsync(int id);

    Task<Usuario?> ObtenerPorNombreUsuarioAsync(
        string nombreUsuario);

    Task AgregarAsync(Usuario usuario);
    Task<bool> ExisteAdministradorAsync();
}
