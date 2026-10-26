
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Domain.Entities
{
    public abstract class DocumentoComercial
    {
        public int Id { get; private set; }

        public string Numero { get; private set; } = string.Empty;

        public DateTime FechaEmision { get; private set; }

        public EstadoDocumento Estado { get; private set; }

        public string? Observaciones { get; private set; }

        protected DocumentoComercial()
        {
        }

        protected DocumentoComercial(
            string numero,
            string? observaciones = null)
        {
            if (string.IsNullOrWhiteSpace(numero))
            {
                throw new ArgumentException(
                    "El número del documento es obligatorio.",
                    nameof(numero));
            }

            Numero = numero.Trim();
            FechaEmision = DateTime.UtcNow;
            Estado = EstadoDocumento.Emitido;
            Observaciones = observaciones;
        }

        public virtual void Anular()
        {
            if (Estado == EstadoDocumento.Anulado)
            {
                throw new InvalidOperationException(
                    "El documento ya se encuentra anulado.");
            }

            Estado = EstadoDocumento.Anulado;
        }
    }
}
