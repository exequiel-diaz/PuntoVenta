
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;
using Xunit;

namespace SistemaVenta.Application.Tests.DocumentosComerciales
{
    public class NotaDebitoTests
    {
        [Fact]
        public void CrearNotaDebito_ConDatosValidos_DebeInicializarCorrectamente()
        {
            var nota = new NotaDebito(
                facturaId: 1,
                numero: "0001-00000004",
                motivo: "Diferencia de precio",
                importeTotal: 15000m);

            Assert.Equal(1, nota.FacturaId);
            Assert.Equal("0001-00000004", nota.Numero);
            Assert.Equal("Diferencia de precio", nota.Motivo);
            Assert.Equal(15000m, nota.ImporteTotal);
            Assert.Equal(EstadoDocumento.Emitido, nota.Estado);
        }

        [Fact]
        public void CrearNotaDebito_ConFacturaInvalida_DebeLanzarExcepcion()
        {
            Assert.Throws<ArgumentException>(() =>
                new NotaDebito(
                    0,
                    "0001-00000004",
                    "Diferencia de precio",
                    15000m));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void CrearNotaDebito_ConMotivoInvalido_DebeLanzarExcepcion(
            string? motivo)
        {
            Assert.Throws<ArgumentException>(() =>
                new NotaDebito(
                    1,
                    "0001-00000004",
                    motivo!,
                    15000m));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-100)]
        public void CrearNotaDebito_ConImporteInvalido_DebeLanzarExcepcion(
            int importe)
        {
            Assert.Throws<ArgumentException>(() =>
                new NotaDebito(
                    1,
                    "0001-00000004",
                    "Diferencia de precio",
                    importe));
        }

        [Fact]
        public void CrearNotaDebito_DebeNormalizarMotivo()
        {
            var nota = new NotaDebito(
                1,
                "0001-00000004",
                "  Diferencia de precio  ",
                15000m);

            Assert.Equal(
                "Diferencia de precio",
                nota.Motivo);
        }
    }
}
