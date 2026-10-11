using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Application.DTOs.Clientes
{
    public class ClienteListadoDto
    {
        public int Id { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Documento { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public EstadoCliente Estado { get; set; }
    }
}
