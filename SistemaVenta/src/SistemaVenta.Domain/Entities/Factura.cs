
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Domain.Entities
{
    public class Factura : DocumentoComercial
    {
        public int VentaId { get; private set; }

        public TipoFactura Tipo { get; private set; }

        public decimal ImporteTotal { get; private set; }

        private Factura()
        {
        }

        public Factura(
            int ventaId,
            TipoFactura tipo,
            decimal importeTotal,
            string numero,
            string? observaciones = null)
            : base(numero, observaciones)
        {
            if (ventaId <= 0)
            {
                throw new ArgumentException(
                    "La venta debe ser válida.",
                    nameof(ventaId));
            }

            if (!Enum.IsDefined(typeof(TipoFactura), tipo))
            {
                throw new ArgumentException(
                    "El tipo de factura no es válido.",
                    nameof(tipo));
            }

            if (importeTotal <= 0)
            {
                throw new ArgumentException(
                    "El importe de la factura debe ser mayor que cero.",
                    nameof(importeTotal));
            }

            VentaId = ventaId;
            Tipo = tipo;
            ImporteTotal = importeTotal;
        }
    }
}
