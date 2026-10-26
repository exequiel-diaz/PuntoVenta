
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Application.UseCases.Usuarios;

public class AutenticarUsuario
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordHasher _passwordHasher;

    public AutenticarUsuario(
        IUsuarioRepository usuarioRepository,
        IPasswordHasher passwordHasher)
    {
        _usuarioRepository = usuarioRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Usuario?> EjecutarAsync(
        string nombreUsuario,
        string password)
    {
        if (string.IsNullOrWhiteSpace(nombreUsuario) ||
            string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        var usuario = await _usuarioRepository
            .ObtenerPorNombreUsuarioAsync(nombreUsuario.Trim());

        if (usuario is null)
            return null;

        if (!usuario.Activo)
            return null;

        var passwordCorrecta = _passwordHasher
            .VerificarPassword(password, usuario.PasswordHash);

        if (!passwordCorrecta)
            return null;

        return usuario;
    }
}
