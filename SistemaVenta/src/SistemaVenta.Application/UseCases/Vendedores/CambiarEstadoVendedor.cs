
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Application.UseCases.Vendedores;

public class CambiarEstadoVendedor
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CambiarEstadoVendedor(
        IUsuarioRepository usuarioRepository,
        IUnitOfWork unitOfWork)
    {
        _usuarioRepository = usuarioRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task EjecutarAsync(int id, bool activar)
    {
        var usuario = await _usuarioRepository
            .ObtenerPorIdAsync(id);

        if (usuario is null)
            throw new InvalidOperationException(
                "El vendedor no existe.");

        if (usuario.Rol != RolUsuario.Vendedor)
            throw new InvalidOperationException(
                "El usuario seleccionado no es un vendedor.");

        if (activar)
        {
            usuario.Activar();
        }
        else
        {
            usuario.Desactivar();
        }

        await _usuarioRepository.ActualizarAsync(usuario);
        await _unitOfWork.GuardarCambiosAsync();
    }
}
