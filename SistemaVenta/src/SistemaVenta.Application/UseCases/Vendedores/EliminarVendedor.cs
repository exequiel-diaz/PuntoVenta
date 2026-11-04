
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Application.UseCases.Vendedores;

public class EliminarVendedor
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IVentaRepository _ventaRepository;
    private readonly IPagoRepository _pagoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EliminarVendedor(
        IUsuarioRepository usuarioRepository,
        IVentaRepository ventaRepository,
        IPagoRepository pagoRepository,
        IUnitOfWork unitOfWork)
    {
        _usuarioRepository = usuarioRepository;
        _ventaRepository = ventaRepository;
        _pagoRepository = pagoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task EjecutarAsync(int id)
    {
        var usuario = await _usuarioRepository
            .ObtenerPorIdAsync(id);

        if (usuario is null)
            throw new InvalidOperationException(
                "El vendedor no existe.");

        if (usuario.Rol != RolUsuario.Vendedor)
            throw new InvalidOperationException(
                "El usuario seleccionado no es un vendedor.");

        bool tieneVentas = await _ventaRepository
            .TieneVentasPorVendedorAsync(id);

        if (tieneVentas)
            throw new InvalidOperationException(
                "No se puede eliminar el vendedor porque tiene ventas registradas.");

        bool tienePagos = await _pagoRepository
            .TienePagosPorUsuarioAsync(id);

        if (tienePagos)
            throw new InvalidOperationException(
                "No se puede eliminar el vendedor porque tiene pagos registrados.");

        await _usuarioRepository.EliminarAsync(usuario);
        await _unitOfWork.GuardarCambiosAsync();
    }
}
