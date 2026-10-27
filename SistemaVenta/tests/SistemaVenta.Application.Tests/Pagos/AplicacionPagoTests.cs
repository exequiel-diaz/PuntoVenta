
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;
using Xunit;

namespace SistemaVenta.Application.Tests.Pagos
{
    public class AplicacionPagoTests
    {
        [Fact]
        public void AgregarAplicacion_Valida_DebeRegistrarse()
        {
            var pago = new Pago(
                100000m,
                FormaPago.Efectivo,
                1);

            pago.AgregarAplicacion(10, 40000m);

            var aplicacion = Assert.Single(pago.Aplicaciones);

            Assert.Same(pago, aplicacion.Pago);
            Assert.Equal(10, aplicacion.DocumentoComercialId);
            Assert.Equal(40000m, aplicacion.ImporteAplicado);
            Assert.Equal(60000m, pago.ImporteDisponible);
        }

        [Fact]
        public void AgregarVariasAplicaciones_DebeActualizarDisponible()
        {
            var pago = new Pago(
                100000m,
                FormaPago.Transferencia,
                1);

            pago.AgregarAplicacion(10, 30000m);
            pago.AgregarAplicacion(11, 50000m);

            Assert.Equal(2, pago.Aplicaciones.Count);
            Assert.Equal(20000m, pago.ImporteDisponible);
        }

        [Fact]
        public void AgregarAplicacion_PorTodoElImporte_DebeDejarDisponibleCero()
        {
            var pago = new Pago(
                100000m,
                FormaPago.Efectivo,
                1);

            pago.AgregarAplicacion(10, 100000m);

            Assert.Equal(0m, pago.ImporteDisponible);
        }

        [Fact]
        public void AgregarAplicacion_SuperandoDisponible_DebeLanzarExcepcion()
        {
            var pago = new Pago(
                100000m,
                FormaPago.Efectivo,
                1);

            pago.AgregarAplicacion(10, 80000m);

            Assert.Throws<InvalidOperationException>(() =>
                pago.AgregarAplicacion(11, 30000m));

            Assert.Single(pago.Aplicaciones);
            Assert.Equal(20000m, pago.ImporteDisponible);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void AgregarAplicacion_ConDocumentoInvalido_DebeLanzarExcepcion(
            int documentoId)
        {
            var pago = new Pago(
                100000m,
                FormaPago.Efectivo,
                1);

            Assert.Throws<ArgumentException>(() =>
                pago.AgregarAplicacion(documentoId, 50000m));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-100)]
        public void AgregarAplicacion_ConImporteInvalido_DebeLanzarExcepcion(
            int importe)
        {
            var pago = new Pago(
                100000m,
                FormaPago.Efectivo,
                1);

            Assert.Throws<ArgumentException>(() =>
                pago.AgregarAplicacion(10, importe));
        }

        [Fact]
        public void AgregarAplicacion_EnPagoAnulado_DebeLanzarExcepcion()
        {
            var pago = new Pago(
                100000m,
                FormaPago.Efectivo,
                1);

            pago.Anular();

            Assert.Throws<InvalidOperationException>(() =>
                pago.AgregarAplicacion(10, 50000m));
        }

        [Fact]
        public void AnularPago_ConAplicaciones_DebeLanzarExcepcion()
        {
            var pago = new Pago(
                100000m,
                FormaPago.Efectivo,
                1);

            pago.AgregarAplicacion(10, 50000m);

            Assert.Throws<InvalidOperationException>(() =>
                pago.Anular());

            Assert.Equal(EstadoPago.Registrado, pago.Estado);
        }
    }
}
