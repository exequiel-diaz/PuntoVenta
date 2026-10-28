
namespace SistemaVenta.Application.DTOs.Ventas
{
    public enum ModalidadVenta
    {
        Contado = 1,
        CuentaCorriente = 2
    }

    public class RegistrarVentaDto
    {
        public int ClienteId { get; set; }

        public ModalidadVenta Modalidad { get; set; }
            = ModalidadVenta.Contado;

        public List<DetalleVentaEntradaDto> Detalles { get; set; }
            = new();
    }

    public class DetalleVentaEntradaDto
    {
        public int ProductoId { get; set; }

        public decimal Cantidad { get; set; }
    }
}
