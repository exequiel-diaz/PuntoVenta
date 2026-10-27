
namespace SistemaVenta.Domain.Entities
{
    public class AplicacionPago
    {
        public int Id { get; private set; }

        public int PagoId { get; private set; }

        public int DocumentoComercialId { get; private set; }

        public decimal ImporteAplicado { get; private set; }

        private AplicacionPago()
        {
        }

        public AplicacionPago(
            int pagoId,
            int documentoComercialId,
            decimal importeAplicado)
        {
            if (pagoId <= 0)
            {
                throw new ArgumentException(
                    "El pago debe ser válido.",
                    nameof(pagoId));
            }

            if (documentoComercialId <= 0)
            {
                throw new ArgumentException(
                    "El documento comercial debe ser válido.",
                    nameof(documentoComercialId));
            }

            if (importeAplicado <= 0)
            {
                throw new ArgumentException(
                    "El importe aplicado debe ser mayor que cero.",
                    nameof(importeAplicado));
            }

            PagoId = pagoId;
            DocumentoComercialId = documentoComercialId;
            ImporteAplicado = importeAplicado;
        }
    }
}
