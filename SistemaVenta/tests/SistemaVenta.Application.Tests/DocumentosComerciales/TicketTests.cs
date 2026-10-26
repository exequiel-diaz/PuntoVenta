
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;
using Xunit;

namespace SistemaVenta.Application.Tests.DocumentosComerciales
{
    public class TicketTests
    {
        [Fact]
        public void CrearTicketConsumidorFinal_ConVentaValida_DebeInicializarCorrectamente()
        {
            var ticket = new Ticket(
                ventaId: 1,
                tipo: TipoTicket.ConsumidorFinal,
                importeTotal: 150000m,
                numero: "0001-00000010");

            Assert.Equal(1, ticket.VentaId);
            Assert.Equal(TipoTicket.ConsumidorFinal, ticket.Tipo);
            Assert.Equal(150000m, ticket.ImporteTotal);
            Assert.Equal(EstadoDocumento.Emitido, ticket.Estado);
        }

        [Fact]
        public void CrearCotizacion_SinVenta_DebeInicializarCorrectamente()
        {
            var ticket = new Ticket(
                ventaId: null,
                tipo: TipoTicket.Cotizacion,
                importeTotal: 200000m,
                numero: "COT-000001");

            Assert.Null(ticket.VentaId);
            Assert.Equal(TipoTicket.Cotizacion, ticket.Tipo);
            Assert.Equal(200000m, ticket.ImporteTotal);
        }

        [Fact]
        public void CrearTicketConsumidorFinal_SinVenta_DebeLanzarExcepcion()
        {
            Assert.Throws<ArgumentException>(() =>
                new Ticket(
                    null,
                    TipoTicket.ConsumidorFinal,
                    150000m,
                    "0001-00000010"));
        }

        [Fact]
        public void CrearCotizacion_ConVenta_DebeLanzarExcepcion()
        {
            Assert.Throws<ArgumentException>(() =>
                new Ticket(
                    1,
                    TipoTicket.Cotizacion,
                    200000m,
                    "COT-000001"));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-100)]
        public void CrearTicket_ConImporteInvalido_DebeLanzarExcepcion(
            int importe)
        {
            Assert.Throws<ArgumentException>(() =>
                new Ticket(
                    1,
                    TipoTicket.ConsumidorFinal,
                    importe,
                    "0001-00000010"));
        }

        [Fact]
        public void CrearTicket_ConTipoInvalido_DebeLanzarExcepcion()
        {
            Assert.Throws<ArgumentException>(() =>
                new Ticket(
                    1,
                    (TipoTicket)999,
                    150000m,
                    "0001-00000010"));
        }
    }
}
