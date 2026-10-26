
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;
using Xunit;

namespace SistemaVenta.Application.Tests.DocumentosComerciales
{
    public class DocumentoComercialTests
    {
        private sealed class DocumentoPrueba : DocumentoComercial
        {
            public DocumentoPrueba(
                string numero,
                string? observaciones = null)
                : base(numero, observaciones)
            {
            }
        }

        [Fact]
        public void CrearDocumento_ConNumeroValido_DebeInicializarCorrectamente()
        {
            // Arrange y Act
            var documento = new DocumentoPrueba(
                "0001-00000001",
                "Documento de prueba");

            // Assert
            Assert.Equal("0001-00000001", documento.Numero);
            Assert.Equal(
                EstadoDocumento.Emitido,
                documento.Estado);
            Assert.Equal(
                "Documento de prueba",
                documento.Observaciones);
            Assert.True(documento.FechaEmision > DateTime.MinValue);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void CrearDocumento_ConNumeroInvalido_DebeLanzarExcepcion(
            string? numero)
        {
            Assert.Throws<ArgumentException>(() =>
                new DocumentoPrueba(numero!));
        }

        [Fact]
        public void CrearDocumento_DebeEliminarEspaciosDelNumero()
        {
            var documento = new DocumentoPrueba(
                " 0001-00000001 ");

            Assert.Equal(
                "0001-00000001",
                documento.Numero);
        }

        [Fact]
        public void AnularDocumento_Emitido_DebeCambiarEstado()
        {
            var documento = new DocumentoPrueba(
                "0001-00000001");

            documento.Anular();

            Assert.Equal(
                EstadoDocumento.Anulado,
                documento.Estado);
        }

        [Fact]
        public void AnularDocumento_DosVeces_DebeLanzarExcepcion()
        {
            var documento = new DocumentoPrueba(
                "0001-00000001");

            documento.Anular();

            Assert.Throws<InvalidOperationException>(() =>
                documento.Anular());
        }
    }
}
