
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;
using Xunit;

namespace SistemaVenta.Application.Tests.DocumentosComerciales
{
    public class NotaCreditoTests
    {
        [Fact]
        public void CrearNotaCredito_ConDatosValidos_DebeInicializarCorrectamente()
        {
            var nota = new NotaCredito(
                facturaId: 1,
                numero: "0001-00000003",
                motivo: "Devolución de teclado",
                importeTotal: 50000m);

            Assert.Equal(1, nota.FacturaId);
            Assert.Equal("0001-00000003", nota.Numero);
            Assert.Equal("Devolución de teclado", nota.Motivo);
            Assert.Equal(50000m, nota.ImporteTotal);
            Assert.Equal(EstadoDocumento.Emitido, nota.Estado);
        }

        [Fact]
        public void CrearNotaCredito_ConFacturaInvalida_DebeLanzarExcepcion()
        {
            Assert.Throws<ArgumentException>(() =>
                new NotaCredito(
                    0,
                    "0001-00000003",
                    "Devolución",
                    50000m));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void CrearNotaCredito_ConMotivoInvalido_DebeLanzarExcepcion(
            string? motivo)
        {
            Assert.Throws<ArgumentException>(() =>
                new NotaCredito(
                    1,
                    "0001-00000003",
                    motivo!,
                    50000m));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-100)]
        public void CrearNotaCredito_ConImporteInvalido_DebeLanzarExcepcion(
            int importe)
        {
            Assert.Throws<ArgumentException>(() =>
                new NotaCredito(
                    1,
                    "0001-00000003",
                    "Devolución",
                    importe));
        }

        [Fact]
        public void CrearNotaCredito_DebeNormalizarMotivo()
        {
            var nota = new NotaCredito(
                1,
                "0001-00000003",
                "  Devolución de teclado  ",
                50000m);

            Assert.Equal(
                "Devolución de teclado",
                nota.Motivo);
        }
    }
}
