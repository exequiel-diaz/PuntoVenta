
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Application.Tests.Fakes;
using SistemaVenta.Application.UseCases.Pagos;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;
using Xunit;

namespace SistemaVenta.Application.Tests.Pagos;

public class RegistrarPagoTests
{
    [Fact]
    public async Task RegistrarPago_Valido_DebeAgregarPagoYGuardarCambios()
    {
        // Arrange
        var repositorio = new FakePagoRepository();
        var unitOfWork = new FakeUnitOfWork();

        var casoDeUso = new RegistrarPago(
            repositorio,
            unitOfWork);

        // Act
        await casoDeUso.EjecutarAsync(
            15000m,
            FormaPago.Efectivo,
            1,
            "Pago de prueba");

        // Assert
        var pago = Assert.Single(repositorio.Pagos);

        Assert.Equal(15000m, pago.Importe);
        Assert.Equal(FormaPago.Efectivo, pago.FormaPago);
        Assert.Equal(EstadoPago.Registrado, pago.Estado);
        Assert.Equal(1, pago.UsuarioId);
        Assert.Equal("Pago de prueba", pago.Observaciones);

        Assert.True(unitOfWork.GuardarCambiosFueLlamado);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public async Task RegistrarPago_ImporteInvalido_DebeLanzarExcepcion(
        decimal importe)
    {
        // Arrange
        var repositorio = new FakePagoRepository();
        var unitOfWork = new FakeUnitOfWork();

        var casoDeUso = new RegistrarPago(
            repositorio,
            unitOfWork);

        // Act y Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => casoDeUso.EjecutarAsync(
                importe,
                FormaPago.Efectivo,
                1));

        Assert.Empty(repositorio.Pagos);
        Assert.False(unitOfWork.GuardarCambiosFueLlamado);
    }

    [Fact]
    public async Task RegistrarPago_FormaPagoInvalida_DebeLanzarExcepcion()
    {
        // Arrange
        var repositorio = new FakePagoRepository();
        var unitOfWork = new FakeUnitOfWork();

        var casoDeUso = new RegistrarPago(
            repositorio,
            unitOfWork);

        // Act y Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => casoDeUso.EjecutarAsync(
                15000m,
                (FormaPago)999,
                1));

        Assert.Empty(repositorio.Pagos);
        Assert.False(unitOfWork.GuardarCambiosFueLlamado);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task RegistrarPago_UsuarioInvalido_DebeLanzarExcepcion(
        int usuarioId)
    {
        // Arrange
        var repositorio = new FakePagoRepository();
        var unitOfWork = new FakeUnitOfWork();

        var casoDeUso = new RegistrarPago(
            repositorio,
            unitOfWork);

        // Act y Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => casoDeUso.EjecutarAsync(
                15000m,
                FormaPago.Efectivo,
                usuarioId));

        Assert.Empty(repositorio.Pagos);
        Assert.False(unitOfWork.GuardarCambiosFueLlamado);
    }


    [Fact]
    public async Task RegistrarPago_SiRepositorioFalla_NoDebeGuardarCambios()
    {
        // Arrange
        var repositorio = new PagoRepositoryConError();
        var unitOfWork = new FakeUnitOfWork();

        var casoDeUso = new RegistrarPago(
            repositorio,
            unitOfWork);

        // Act y Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => casoDeUso.EjecutarAsync(
                15000m,
                FormaPago.Efectivo,
                1));

        Assert.False(unitOfWork.GuardarCambiosFueLlamado);
    }


    internal class PagoRepositoryConError : IPagoRepository
    {
        public Task AgregarAsync(
            Pago pago,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "Error simulado al agregar el pago.");
        }

        public Task<Pago?> ObtenerPorIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Pago?>(null);
        }
    }

}
