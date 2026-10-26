
namespace SistemaVenta.Domain.Entities
{
    public class NotaDebito : DocumentoComercial
    {
        public int FacturaId { get; private set; }

        public string Motivo { get; private set; } = string.Empty;

        public decimal ImporteTotal { get; private set; }

        private NotaDebito()
        {
        }

        public NotaDebito(
            int facturaId,
            string numero,
            string motivo,
            decimal importeTotal,
            string? observaciones = null)
            : base(numero, observaciones)
        {
            if (facturaId <= 0)
            {
                throw new ArgumentException(
                    "La factura asociada debe ser válida.",
                    nameof(facturaId));
            }

            if (string.IsNullOrWhiteSpace(motivo))
            {
                throw new ArgumentException(
                    "El motivo de la nota de débito es obligatorio.",
                    nameof(motivo));
            }

            if (importeTotal <= 0)
            {
                throw new ArgumentException(
                    "El importe de la nota de débito debe ser mayor que cero.",
                    nameof(importeTotal));
            }

            FacturaId = facturaId;
            Motivo = motivo.Trim();
            ImporteTotal = importeTotal;
        }
    }
}
