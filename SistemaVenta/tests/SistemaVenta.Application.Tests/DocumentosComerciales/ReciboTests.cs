
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;
using Xunit;

namespace SistemaVenta.Application.Tests.DocumentosComerciales
{
    public class ReciboTests
    {
        [Fact]
        public void CrearRecibo_ConDatosValidos_DebeInicializarCorrectamente()
        {
            var recibo = new Recibo(
                pagoId: 1,
                numero: "REC-000001",
                importeTotal: 300000m);

            Assert.Equal(1, recibo.PagoId);
            Assert.Equal("REC-000001", recibo.Numero);
            Assert.Equal(300000m, recibo.ImporteTotal);
            Assert.Equal(EstadoDocumento.Emitido, recibo.Estado);
            Assert.True(recibo.FechaEmision > DateTime.MinValue);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void CrearRecibo_ConPagoInvalido_DebeLanzarExcepcion(
            int pagoId)
        {
            Assert.Throws<ArgumentException>(() =>
                new Recibo(
                    pagoId,
                    "REC-000001",
                    300000m));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-100)]
        public void CrearRecibo_ConImporteInvalido_DebeLanzarExcepcion(
            int importe)
        {
            Assert.Throws<ArgumentException>(() =>
                new Recibo(
                    1,
                    "REC-000001",
                    importe));
        }

        [Fact]
        public void CrearRecibo_ConNumeroVacio_DebeLanzarExcepcion()
        {
            Assert.Throws<ArgumentException>(() =>
                new Recibo(
                    1,
                    "",
                    300000m));
        }

        [Fact]
        public void CrearRecibo_DebeConservarObservaciones()
        {
            var recibo = new Recibo(
                1,
                "REC-000001",
                300000m,
                "Pago parcial de notebook");

            Assert.Equal(
                "Pago parcial de notebook",
                recibo.Observaciones);
        }
    }
}
