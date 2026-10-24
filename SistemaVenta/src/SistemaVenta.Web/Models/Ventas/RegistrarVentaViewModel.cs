
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SistemaVenta.Web.ViewModels.Ventas;

public class RegistrarVentaViewModel
{
    [Range(1, int.MaxValue,
        ErrorMessage = "Debe seleccionar un cliente.")]
    public int ClienteId { get; set; }

    [Required(ErrorMessage = "Debe agregar productos.")]
    [MinLength(1,
        ErrorMessage = "Debe agregar al menos un producto.")]
    public List<DetalleVentaViewModel> Detalles { get; set; } = new();

    // Opciones para los desplegables.
    // No se envían como parte de la venta.
    public List<SelectListItem> ClientesDisponibles { get; set; } = new();

    public List<SelectListItem> ProductosDisponibles { get; set; } = new();

    public Dictionary<int, decimal> PreciosProductos { get; set; } = new();

}
