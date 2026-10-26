
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Domain.Entities
{
    public class Ticket : DocumentoComercial
    {
        public int? VentaId { get; private set; }

        public TipoTicket Tipo { get; private set; }

        public decimal ImporteTotal { get; private set; }

        private Ticket()
        {
        }

        public Ticket(
            int? ventaId,
            TipoTicket tipo,
            decimal importeTotal,
            string numero,
            string? observaciones = null)
            : base(numero, observaciones)
        {
            if (!Enum.IsDefined(typeof(TipoTicket), tipo))
            {
                throw new ArgumentException(
                    "El tipo de ticket no es válido.",
                    nameof(tipo));
            }

            if (importeTotal <= 0)
            {
                throw new ArgumentException(
                    "El importe del ticket debe ser mayor que cero.",
                    nameof(importeTotal));
            }

            if (tipo == TipoTicket.ConsumidorFinal &&
                (!ventaId.HasValue || ventaId.Value <= 0))
            {
                throw new ArgumentException(
                    "Un ticket de consumidor final debe estar asociado a una venta.",
                    nameof(ventaId));
            }

            if (tipo == TipoTicket.Cotizacion &&
                ventaId.HasValue)
            {
                throw new ArgumentException(
                    "Una cotización no debe estar asociada a una venta confirmada.",
                    nameof(ventaId));
            }

            VentaId = ventaId;
            Tipo = tipo;
            ImporteTotal = importeTotal;
        }
    }
}
