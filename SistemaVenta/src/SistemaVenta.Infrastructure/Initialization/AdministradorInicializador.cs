
using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Application.UseCases.Usuarios;
using SistemaVenta.Domain.Enums;
using SistemaVenta.Infrastructure.Persistence;

namespace SistemaVenta.Infrastructure.Initialization;

public class AdministradorInicializador
{
    private readonly AppDbContext _context;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly RegistrarUsuario _registrarUsuario;
    private readonly IConfiguration _configuration;

    public AdministradorInicializador(
        AppDbContext context,
        IUsuarioRepository usuarioRepository,
        RegistrarUsuario registrarUsuario,
        IConfiguration configuration)
    {
        _context = context;
        _usuarioRepository = usuarioRepository;
        _registrarUsuario = registrarUsuario;
        _configuration = configuration;
    }

    public async Task InicializarAsync()
    {
        string nombreUsuario =
            _configuration["AdministradorInicial:NombreUsuario"]
            ?? throw new InvalidOperationException(
                "Falta configurar el nombre de usuario inicial.");

        string nombreCompleto =
            _configuration["AdministradorInicial:NombreCompleto"]
            ?? throw new InvalidOperationException(
                "Falta configurar el nombre completo inicial.");

        string password =
            _configuration["AdministradorInicial:Password"]
            ?? throw new InvalidOperationException(
                "Falta configurar la contraseña inicial.");

        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        bool existeAdministrador =
            await _usuarioRepository.ExisteAdministradorAsync();

        if (existeAdministrador)
        {
            throw new InvalidOperationException(
                "Ya existe un administrador. " +
                "No se permite ejecutar nuevamente la inicialización.");
        }

        await _registrarUsuario.EjecutarAsync(
            nombreUsuario,
            nombreCompleto,
            password,
            RolUsuario.Administrador);

        await transaction.CommitAsync();
    }
}
