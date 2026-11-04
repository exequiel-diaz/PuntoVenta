
using SistemaVenta.Application.DTOs.Vendedores;
using SistemaVenta.Application.Interfaces;

namespace SistemaVenta.Application.UseCases.Vendedores;

public class ObtenerVendedores
{
    private readonly IUsuarioRepository _usuarioRepository;

    public ObtenerVendedores(
        IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    public async Task<List<VendedorDto>> EjecutarAsync()
    {
        // Obtenemos los vendedores desde el repositorio.
        var vendedores = await _usuarioRepository
            .ObtenerVendedoresAsync();

        // Convertimos las entidades a DTOs.
        return vendedores
            .Select(v => new VendedorDto
            {
                Id = v.Id,
                NombreUsuario = v.NombreUsuario,
                NombreCompleto = v.NombreCompleto,
                Activo = v.Activo
            })
            .ToList();
    }
}
