
namespace SistemaVenta.Application.DTOs.Informes
{
    public class VentaPorVendedorDto
    {
        public int? VendedorId { get; set; }

        public string NombreVendedor { get; set; } = string.Empty;

        public string Rol { get; set; } = string.Empty;

        public bool? Activo { get; set; }

        public int CantidadVentas { get; set; }

        public decimal TotalVendido { get; set; }
    }
}
