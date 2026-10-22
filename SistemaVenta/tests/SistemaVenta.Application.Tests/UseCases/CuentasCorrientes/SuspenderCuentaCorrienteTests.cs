using SistemaVenta.Application.Tests.Fakes;
using SistemaVenta.Application.UseCases.CuentasCorrientes;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;
using Xunit;

namespace SistemaVenta.Application.Tests.UseCases.CuentasCorrientes;

public class SuspenderCuentaCorrienteTests
{
    [Fact]
    public async Task SuspenderCuenta_DebeCambiarEstadoYGuardarCambios()
    {
        // Arrange
        var repositorio = new FakeCuentaCorrienteRepository();
        var unitOfWork = new FakeUnitOfWork();

        var cuenta = new CuentaCorriente(
            clienteId: 1,
            limiteDeuda: 500000m);

        repositorio.ConfigurarCuenta(1, cuenta);

        var casoDeUso = new SuspenderCuentaCorriente(
            repositorio,
            unitOfWork);

        // Act
        await casoDeUso.EjecutarAsync(1);

        // Assert
        Assert.Equal(
            EstadoCuentaCorriente.Suspendida,
            cuenta.Estado);

        Assert.True(unitOfWork.GuardarCambiosFueLlamado);
    }

    [Fact]
    public async Task SuspenderCuenta_Inexistente_DebeFallar()
    {
        // Arrange
        var repositorio = new FakeCuentaCorrienteRepository();
        var unitOfWork = new FakeUnitOfWork();

        var casoDeUso = new SuspenderCuentaCorriente(
            repositorio,
            unitOfWork);

        // Act y Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => casoDeUso.EjecutarAsync(999));

        Assert.False(unitOfWork.GuardarCambiosFueLlamado);
    }
}