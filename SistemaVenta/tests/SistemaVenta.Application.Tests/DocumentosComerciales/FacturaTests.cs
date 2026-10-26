
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;
using Xunit;

namespace SistemaVenta.Application.Tests.DocumentosComerciales
{
    public class FacturaTests
    {
        [Fact]
        public void CrearFactura_ConDatosValidos_DebeInicializarCorrectamente()
        {
            var factura = new Factura(
                ventaId: 1,
                tipo: TipoFactura.B,
                importeTotal: 450000m,
                numero: "0001-00000001");

            Assert.Equal(1, factura.VentaId);
            Assert.Equal(TipoFactura.B, factura.Tipo);
            Assert.Equal(450000m, factura.ImporteTotal);
            Assert.Equal("0001-00000001", factura.Numero);
            Assert.Equal(EstadoDocumento.Emitido, factura.Estado);
        }

        [Fact]
        public void CrearFactura_ConVentaInvalida_DebeLanzarExcepcion()
        {
            Assert.Throws<ArgumentException>(() =>
                new Factura(
                    0,
                    TipoFactura.B,
                    450000m,
                    "0001-00000001"));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-100)]
        public void CrearFactura_ConImporteInvalido_DebeLanzarExcepcion(
            int importe)
        {
            Assert.Throws<ArgumentException>(() =>
                new Factura(
                    1,
                    TipoFactura.B,
                    importe,
                    "0001-00000001"));
        }

        [Fact]
        public void CrearFactura_ConTipoInvalido_DebeLanzarExcepcion()
        {
            Assert.Throws<ArgumentException>(() =>
                new Factura(
                    1,
                    (TipoFactura)999,
                    450000m,
                    "0001-00000001"));
        }

        [Fact]
        public void CrearFactura_ConNumeroVacio_DebeLanzarExcepcion()
        {
            Assert.Throws<ArgumentException>(() =>
                new Factura(
                    1,
                    TipoFactura.B,
                    450000m,
                    ""));
        }
    }
}
