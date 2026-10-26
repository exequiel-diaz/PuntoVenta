
namespace SistemaVenta.Domain.Entities
{
    public class Recibo : DocumentoComercial
    {
        public int PagoId { get; private set; }

        public decimal ImporteTotal { get; private set; }

        private Recibo()
        {
        }

        public Recibo(
            int pagoId,
            string numero,
            decimal importeTotal,
            string? observaciones = null)
            : base(numero, observaciones)
        {
            if (pagoId <= 0)
            {
                throw new ArgumentException(
                    "El pago asociado debe ser válido.",
                    nameof(pagoId));
            }

            if (importeTotal <= 0)
            {
                throw new ArgumentException(
                    "El importe del recibo debe ser mayor que cero.",
                    nameof(importeTotal));
            }

            PagoId = pagoId;
            ImporteTotal = importeTotal;
        }
    }
}
