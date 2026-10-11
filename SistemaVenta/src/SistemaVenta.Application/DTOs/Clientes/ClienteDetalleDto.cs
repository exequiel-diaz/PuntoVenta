using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Application.DTOs.Clientes
{
    public class ClienteDetalleDto
    {
        public int Id { get; set; }

        public string Tipo { get; set; } = string.Empty;

        public string? Nombre { get; set; }

        public string? Apellido { get; set; }

        public string? Dni { get; set; }

        public string? RazonSocial { get; set; }

        public string? Cuit { get; set; }

        public string? Contacto { get; set; }

        public DateTime? FechaNacimiento { get; set; }

        public string Telefono { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Direccion { get; set; } = string.Empty;

        public string? Observaciones { get; set; }

        public EstadoCliente Estado { get; set; }
    }
}
