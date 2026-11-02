namespace SistemaVenta.Application.DTOs.Ventas
{
    public class NotaComercialDto
    {
        public string Tipo { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string Motivo { get; set; } = string.Empty;
        public decimal Importe { get; set; }
    }
}