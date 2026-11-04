
using SistemaVenta.Application.DTOs.Vendedores;
using SistemaVenta.Application.UseCases.Usuarios;
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Application.UseCases.Vendedores;

public class CrearVendedor
{
    private readonly RegistrarUsuario _registrarUsuario;

    public CrearVendedor(RegistrarUsuario registrarUsuario)
    {
        _registrarUsuario = registrarUsuario;
    }

    public async Task EjecutarAsync(CrearVendedorDto datos)
    {
        if (datos is null)
            throw new ArgumentNullException(nameof(datos));

        await _registrarUsuario.EjecutarAsync(
            datos.NombreUsuario,
            datos.NombreCompleto,
            datos.Password,
            RolUsuario.Vendedor);
    }
}
