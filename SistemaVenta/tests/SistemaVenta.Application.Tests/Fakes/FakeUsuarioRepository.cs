
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Application.Tests.Fakes;

public class FakeUsuarioRepository : IUsuarioRepository
{
    private readonly List<Usuario> _usuarios = new();

    public IReadOnlyList<Usuario> Usuarios => _usuarios;

    public Task<Usuario?> ObtenerPorIdAsync(int id)
    {
        var usuario = _usuarios.FirstOrDefault(u => u.Id == id);

        return Task.FromResult(usuario);
    }

    public Task<Usuario?> ObtenerPorNombreUsuarioAsync(
        string nombreUsuario)
    {
        var usuario = _usuarios.FirstOrDefault(u =>
            string.Equals(
                u.NombreUsuario,
                nombreUsuario,
                StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(usuario);
    }

    public Task AgregarAsync(Usuario usuario)
    {
        _usuarios.Add(usuario);

        return Task.CompletedTask;
    }

    public Task<bool> ExisteAdministradorAsync()
    {
        return Task.FromResult(
            _usuarios.Any(u =>
                u.Rol == RolUsuario.Administrador));
    }


    public Task ActualizarAsync(Usuario usuario)
    {
        var indice = _usuarios.FindIndex(u => u.Id == usuario.Id);

        if (indice == -1)
            throw new InvalidOperationException(
                "El usuario no existe.");

        _usuarios[indice] = usuario;

        return Task.CompletedTask;
    }


    public Task<List<Usuario>> ObtenerVendedoresAsync()
    {
        var vendedores = _usuarios
            .Where(u => u.Rol == RolUsuario.Vendedor)
            .OrderBy(u => u.NombreCompleto)
            .ToList();

        return Task.FromResult(vendedores);
    }


    public Task EliminarAsync(Usuario usuario)
    {
        ArgumentNullException.ThrowIfNull(usuario);

        var usuarioExistente = _usuarios
            .FirstOrDefault(u => u.Id == usuario.Id);

        if (usuarioExistente == null)
        {
            throw new InvalidOperationException(
                "El usuario no existe.");
        }

        _usuarios.Remove(usuarioExistente);

        return Task.CompletedTask;
    }


}
