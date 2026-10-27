
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;
using Xunit;

namespace SistemaVenta.Application.Tests.Pagos
{
    public class PagoTests
    {
        [Fact]
        public void CrearPago_ConDatosValidos_DebeInicializarCorrectamente()
        {
            var pago = new Pago(
                importe: 250000m,
                formaPago: FormaPago.Transferencia,
                usuarioId: 1,
                observaciones: "Pago parcial");

            Assert.Equal(250000m, pago.Importe);
            Assert.Equal(FormaPago.Transferencia, pago.FormaPago);
            Assert.Equal(1, pago.UsuarioId);
            Assert.Equal(EstadoPago.Registrado, pago.Estado);
            Assert.Equal("Pago parcial", pago.Observaciones);
            Assert.True(pago.Fecha > DateTime.MinValue);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-100)]
        public void CrearPago_ConImporteInvalido_DebeLanzarExcepcion(
            int importe)
        {
            Assert.Throws<ArgumentException>(() =>
                new Pago(
                    importe,
                    FormaPago.Efectivo,
                    1));
        }

        [Fact]
        public void CrearPago_ConFormaInvalida_DebeLanzarExcepcion()
        {
            Assert.Throws<ArgumentException>(() =>
                new Pago(
                    250000m,
                    (FormaPago)999,
                    1));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void CrearPago_ConUsuarioInvalido_DebeLanzarExcepcion(
            int usuarioId)
        {
            Assert.Throws<ArgumentException>(() =>
                new Pago(
                    250000m,
                    FormaPago.Efectivo,
                    usuarioId));
        }

        [Fact]
        public void AnularPago_Registrado_DebeCambiarEstado()
        {
            var pago = new Pago(
                250000m,
                FormaPago.Efectivo,
                1);

            pago.Anular();

            Assert.Equal(EstadoPago.Anulado, pago.Estado);
        }

        [Fact]
        public void AnularPago_DosVeces_DebeLanzarExcepcion()
        {
            var pago = new Pago(
                250000m,
                FormaPago.Efectivo,
                1);

            pago.Anular();

            Assert.Throws<InvalidOperationException>(() =>
                pago.Anular());
        }
    }
}
