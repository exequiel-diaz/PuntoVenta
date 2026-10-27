
namespace SistemaVenta.Domain.Entities
{
    public class AplicacionPago
    {
        public int Id { get; private set; }

        public int PagoId { get; private set; }

        public int DocumentoComercialId { get; private set; }

        public decimal ImporteAplicado { get; private set; }

        public Pago Pago { get; private set; } = null!;

        private AplicacionPago()
        {
        }

        internal AplicacionPago(
            Pago pago,
            int documentoComercialId,
            decimal importeAplicado)
        {
            if (pago is null)
            {
                throw new ArgumentNullException(nameof(pago));
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

            Pago = pago;
            DocumentoComercialId = documentoComercialId;
            ImporteAplicado = importeAplicado;
        }
    }
}
