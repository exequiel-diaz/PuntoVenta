
using System.ComponentModel.DataAnnotations;

namespace SistemaVenta.Web.Models.Clientes;

public class EditarClienteEmpresaViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "La razón social es obligatoria.")]
    public string RazonSocial { get; set; } = string.Empty;

    [Required(ErrorMessage = "El CUIT es obligatorio.")]
    public string Cuit { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ingresá un correo electrónico válido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La dirección es obligatoria.")]
    public string Direccion { get; set; } = string.Empty;

    public string? Contacto { get; set; }

    public string? Observaciones { get; set; }
}
