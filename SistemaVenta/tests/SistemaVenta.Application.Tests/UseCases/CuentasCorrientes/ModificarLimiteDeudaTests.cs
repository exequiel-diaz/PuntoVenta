using SistemaVenta.Application.Tests.Fakes;
using SistemaVenta.Application.UseCases.CuentasCorrientes;
using SistemaVenta.Domain.Entities;
using Xunit;

namespace SistemaVenta.Application.Tests.UseCases.CuentasCorrientes;

public class ModificarLimiteDeudaTests
{
    [Fact]
    public async Task ModificarLimite_DebeActualizarLimiteYGuardarCambios()
    {
        // Arrange
        var repositorio = new FakeCuentaCorrienteRepository();
        var unitOfWork = new FakeUnitOfWork();

        var cuenta = new CuentaCorriente(
            clienteId: 1,
            limiteDeuda: 500000m);

        repositorio.ConfigurarCuenta(1, cuenta);

        var casoDeUso = new ModificarLimiteDeuda(
            repositorio,
            unitOfWork);

        // Act
        await casoDeUso.EjecutarAsync(
            cuentaId: 1,
            nuevoLimite: 300000m);

        // Assert
        Assert.Equal(300000m, cuenta.LimiteDeuda);
        Assert.True(unitOfWork.GuardarCambiosFueLlamado);
    }

    [Fact]
    public async Task ModificarLimite_CuentaInexistente_DebeFallar()
    {
        // Arrange
        var repositorio = new FakeCuentaCorrienteRepository();
        var unitOfWork = new FakeUnitOfWork();

        var casoDeUso = new ModificarLimiteDeuda(
            repositorio,
            unitOfWork);

        // Act y Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => casoDeUso.EjecutarAsync(
                cuentaId: 999,
                nuevoLimite: 300000m));

        Assert.False(unitOfWork.GuardarCambiosFueLlamado);
    }
}