using SistemaVenta.Application.Tests.Fakes;
using SistemaVenta.Application.UseCases.CuentasCorrientes;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;
using Xunit;

namespace SistemaVenta.Application.Tests.UseCases.CuentasCorrientes;

public class CerrarCuentaCorrienteTests
{
    [Fact]
    public async Task CerrarCuentaSinDeuda_DebeCambiarEstado()
    {
        // Arrange
        var repositorio = new FakeCuentaCorrienteRepository();
        var unitOfWork = new FakeUnitOfWork();

        var cuenta = new CuentaCorriente(
            clienteId: 1,
            limiteDeuda: 500000m);

        repositorio.ConfigurarCuenta(1, cuenta);

        var casoDeUso = new CerrarCuentaCorriente(
            repositorio,
            unitOfWork);

        // Act
        await casoDeUso.EjecutarAsync(1);

        // Assert
        Assert.Equal(
            EstadoCuentaCorriente.Cerrada,
            cuenta.Estado);

        Assert.True(unitOfWork.GuardarCambiosFueLlamado);
    }

    [Fact]
    public async Task CerrarCuentaConDeuda_DebeFallar()
    {
        // Arrange
        var repositorio = new FakeCuentaCorrienteRepository();
        var unitOfWork = new FakeUnitOfWork();

        var cuenta = new CuentaCorriente(
            clienteId: 1,
            limiteDeuda: 500000m);

        cuenta.RegistrarDebito(
            100000m,
            "Venta de prueba");

        repositorio.ConfigurarCuenta(1, cuenta);

        var casoDeUso = new CerrarCuentaCorriente(
            repositorio,
            unitOfWork);

        // Act y Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => casoDeUso.EjecutarAsync(1));

        Assert.Equal(
            EstadoCuentaCorriente.Activa,
            cuenta.Estado);

        Assert.False(unitOfWork.GuardarCambiosFueLlamado);
    }
}