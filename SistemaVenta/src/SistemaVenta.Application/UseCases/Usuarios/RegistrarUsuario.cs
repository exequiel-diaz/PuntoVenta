
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Application.UseCases.Usuarios;

public class RegistrarUsuario
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public RegistrarUsuario(
        IUsuarioRepository usuarioRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _usuarioRepository = usuarioRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task EjecutarAsync(
        string nombreUsuario,
        string nombreCompleto,
        string password,
        RolUsuario rol)
    {
        if (string.IsNullOrWhiteSpace(nombreUsuario))
            throw new ArgumentException(
                "El nombre de usuario es obligatorio.");

        if (string.IsNullOrWhiteSpace(nombreCompleto))
            throw new ArgumentException(
                "El nombre completo es obligatorio.");

        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException(
                "La contraseña es obligatoria.");

        if (password.Length < 12)
            throw new ArgumentException(
                "La contraseña debe tener al menos 12 caracteres.");

        if (!Enum.IsDefined(rol))
            throw new ArgumentException(
                "El rol seleccionado no es válido.");

        nombreUsuario = nombreUsuario.Trim();
        nombreCompleto = nombreCompleto.Trim();

        var usuarioExistente =
            await _usuarioRepository.ObtenerPorNombreUsuarioAsync(
                nombreUsuario);

        if (usuarioExistente is not null)
            throw new InvalidOperationException(
                "El nombre de usuario ya está registrado.");

        var passwordHash =
            _passwordHasher.GenerarHash(password);

        var usuario = new Usuario(
            nombreUsuario,
            nombreCompleto,
            passwordHash,
            rol);

        await _usuarioRepository.AgregarAsync(usuario);
        await _unitOfWork.GuardarCambiosAsync();
    }
}
