
namespace SistemaVenta.Web.Models.Vendedores;

public class PermisosVendedorViewModel
{
    public int UsuarioId { get; set; }

    public string NombreCompleto { get; set; }
        = string.Empty;

    public List<string> PermisosSeleccionados { get; set; }
        = new();
}
