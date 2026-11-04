
using SistemaVenta.Application.DTOs.Vendedores;
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Application.UseCases.Vendedores;

public class EditarVendedor
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EditarVendedor(
        IUsuarioRepository usuarioRepository,
        IUnitOfWork unitOfWork)
    {
        _usuarioRepository = usuarioRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task EjecutarAsync(EditarVendedorDto datos)
    {
        if (datos is null)
            throw new ArgumentNullException(nameof(datos));

        // Buscar el usuario que queremos editar.
        var usuario = await _usuarioRepository
            .ObtenerPorIdAsync(datos.Id);

        if (usuario is null)
            throw new InvalidOperationException(
                "El vendedor no existe.");

        // Verificar que sea un vendedor y no un administrador.
        if (usuario.Rol != RolUsuario.Vendedor)
            throw new InvalidOperationException(
                "El usuario seleccionado no es un vendedor.");

        // Comprobar que el nombre de usuario no esté ocupado.
        var usuarioExistente = await _usuarioRepository
            .ObtenerPorNombreUsuarioAsync(datos.NombreUsuario?.Trim()
                ?? string.Empty);

        if (usuarioExistente is not null &&
            usuarioExistente.Id != usuario.Id)
        {
            throw new InvalidOperationException(
                "El nombre de usuario ya está registrado.");
        }

        // Aplicar las reglas de negocio de la entidad.
        usuario.ActualizarDatos(
            datos.NombreUsuario,
            datos.NombreCompleto);

        // Persistir los cambios.
        await _usuarioRepository.ActualizarAsync(usuario);
        await _unitOfWork.GuardarCambiosAsync();
    }
}
