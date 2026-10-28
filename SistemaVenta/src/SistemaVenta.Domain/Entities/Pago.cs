
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Domain.Entities
{
    public class Pago
    {
        public int Id { get; private set; }

        public DateTime Fecha { get; private set; }

        public decimal Importe { get; private set; }

        public FormaPago FormaPago { get; private set; }

        public EstadoPago Estado { get; private set; }

        public string? Observaciones { get; private set; }

        public int UsuarioId { get; private set; }

        private readonly List<AplicacionPago> _aplicaciones = new();

        public IReadOnlyCollection<AplicacionPago> Aplicaciones =>
            _aplicaciones.AsReadOnly();

        private readonly List<AplicacionPagoVenta> _aplicacionesVentas = new();

        public IReadOnlyCollection<AplicacionPagoVenta> AplicacionesVentas =>
            _aplicacionesVentas.AsReadOnly();

        public decimal ImporteDisponible =>
            Importe
            - _aplicaciones.Sum(a => a.ImporteAplicado)
            - _aplicacionesVentas.Sum(a => a.ImporteAplicado);
        private Pago()
        {
        }

        public Pago(
            decimal importe,
            FormaPago formaPago,
            int usuarioId,
            string? observaciones = null)
        {
            if (importe <= 0)
            {
                throw new ArgumentException(
                    "El importe del pago debe ser mayor que cero.",
                    nameof(importe));
            }

            if (!Enum.IsDefined(typeof(FormaPago), formaPago))
            {
                throw new ArgumentException(
                    "La forma de pago no es válida.",
                    nameof(formaPago));
            }

            if (usuarioId <= 0)
            {
                throw new ArgumentException(
                    "El usuario debe ser válido.",
                    nameof(usuarioId));
            }

            Importe = importe;
            FormaPago = formaPago;
            UsuarioId = usuarioId;
            Fecha = DateTime.UtcNow;
            Estado = EstadoPago.Registrado;
            Observaciones = observaciones;
        }

        public void AgregarAplicacion(
            int documentoComercialId,
            decimal importeAplicado)
        {
            if (Estado == EstadoPago.Anulado)
            {
                throw new InvalidOperationException(
                    "No se puede aplicar un pago anulado.");
            }

            if (importeAplicado <= 0)
            {
                throw new ArgumentException(
                    "El importe aplicado debe ser mayor que cero.",
                    nameof(importeAplicado));
            }

            if (importeAplicado > ImporteDisponible)
            {
                throw new InvalidOperationException(
                    "El importe aplicado supera el saldo disponible del pago.");
            }

            var aplicacion = new AplicacionPago(
                this,
                documentoComercialId,
                importeAplicado);

            _aplicaciones.Add(aplicacion);
        }

        public void AgregarAplicacionVenta(
            Venta venta,
            decimal importeAplicado)
        {
            if (Estado == EstadoPago.Anulado)
                throw new InvalidOperationException(
                    "No se puede aplicar un pago anulado.");

            if (venta is null)
                throw new ArgumentNullException(nameof(venta));

            if (venta.Id <= 0)
                throw new ArgumentException(
                    "La venta debe estar registrada.");

            if (importeAplicado <= 0)
                throw new ArgumentException(
                    "El importe aplicado debe ser mayor que cero.");

            if (importeAplicado > ImporteDisponible)
                throw new InvalidOperationException(
                    "El importe aplicado supera el saldo disponible del pago.");

            var aplicacion = new AplicacionPagoVenta(
                this,
                venta,
                importeAplicado);

            _aplicacionesVentas.Add(aplicacion);
        }

        public void Anular()
        {
            if (Estado == EstadoPago.Anulado)
            {
                throw new InvalidOperationException(
                    "El pago ya se encuentra anulado.");
            }

            if (_aplicaciones.Count > 0 || _aplicacionesVentas.Count > 0)
            {
                throw new InvalidOperationException(
                    "No se puede anular un pago que tiene aplicaciones.");
            }

            Estado = EstadoPago.Anulado;
        }
    
    
    }
}
