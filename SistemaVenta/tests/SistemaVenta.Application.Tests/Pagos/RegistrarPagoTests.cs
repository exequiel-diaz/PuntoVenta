
using SistemaVenta.Application.UseCases.Pagos;
using SistemaVenta.Application.Tests.Fakes;
using SistemaVenta.Domain.Enums;
using Xunit;

namespace SistemaVenta.Application.Tests.Pagos
{
    public class RegistrarPagoTests
    {
        [Fact]
        public async Task RegistrarPago_Valido_DebeGuardarPago()
        {
            var repository = new FakePagoRepository();
            var unitOfWork = new FakeUnitOfWork();

            var casoDeUso = new RegistrarPago(
                repository,
                unitOfWork);

            await casoDeUso.EjecutarAsync(
                150000m,
                FormaPago.Transferencia,
                1,
                "Pago parcial");

            var pago = Assert.Single(repository.Pagos);

            Assert.Equal(150000m, pago.Importe);
            Assert.Equal(FormaPago.Transferencia, pago.FormaPago);
            Assert.Equal(1, pago.UsuarioId);
            Assert.Equal("Pago parcial", pago.Observaciones);
            Assert.Equal(EstadoPago.Registrado, pago.Estado);
        }

        [Fact]
        public async Task RegistrarPago_ConImporteInvalido_NoDebeGuardar()
        {
            var repository = new FakePagoRepository();
            var unitOfWork = new FakeUnitOfWork();

            var casoDeUso = new RegistrarPago(
                repository,
                unitOfWork);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                casoDeUso.EjecutarAsync(
                    0m,
                    FormaPago.Efectivo,
                    1));

            Assert.Empty(repository.Pagos);
        }

        [Fact]
        public async Task RegistrarPago_ConFormaInvalida_NoDebeGuardar()
        {
            var repository = new FakePagoRepository();
            var unitOfWork = new FakeUnitOfWork();

            var casoDeUso = new RegistrarPago(
                repository,
                unitOfWork);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                casoDeUso.EjecutarAsync(
                    150000m,
                    (FormaPago)999,
                    1));

            Assert.Empty(repository.Pagos);
        }
    }
}
