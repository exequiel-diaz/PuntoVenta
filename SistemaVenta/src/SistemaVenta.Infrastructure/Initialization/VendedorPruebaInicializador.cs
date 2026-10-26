
using Microsoft.Extensions.Configuration;
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Application.UseCases.Usuarios;
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Infrastructure.Initialization;

public class VendedorPruebaInicializador
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly RegistrarUsuario _registrarUsuario;
    private readonly IConfiguration _configuration;

    public VendedorPruebaInicializador(
        IUsuarioRepository usuarioRepository,
        RegistrarUsuario registrarUsuario,
        IConfiguration configuration)
    {
        _usuarioRepository = usuarioRepository;
        _registrarUsuario = registrarUsuario;
        _configuration = configuration;
    }

    public async Task InicializarAsync()
    {
        string nombreUsuario =
            _configuration["VendedorPrueba:NombreUsuario"]
            ?? throw new InvalidOperationException(
                "Falta configurar el nombre de usuario.");

        string nombreCompleto =
            _configuration["VendedorPrueba:NombreCompleto"]
            ?? throw new InvalidOperationException(
                "Falta configurar el nombre completo.");

        string password =
            _configuration["VendedorPrueba:Password"]
            ?? throw new InvalidOperationException(
                "Falta configurar la contraseña.");

        var usuarioExistente =
            await _usuarioRepository.ObtenerPorNombreUsuarioAsync(
                nombreUsuario);

        if (usuarioExistente is not null)
        {
            throw new InvalidOperationException(
                "El usuario vendedor ya existe.");
        }

        await _registrarUsuario.EjecutarAsync(
            nombreUsuario,
            nombreCompleto,
            password,
            RolUsuario.Vendedor);
    }
}
