
namespace SistemaVenta.Application.DTOs.Vendedores;

public class VendedorDto
{
    public int Id { get; set; }

    public string NombreUsuario { get; set; } = string.Empty;

    public string NombreCompleto { get; set; } = string.Empty;

    public bool Activo { get; set; }
}
