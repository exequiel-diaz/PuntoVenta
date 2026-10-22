using SistemaVenta.Application.Tests.Fakes;
using SistemaVenta.Application.UseCases.CuentasCorrientes;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;
using Xunit;

namespace SistemaVenta.Application.Tests.UseCases.CuentasCorrientes;

public class ActivarCuentaCorrienteTests
{
    [Fact]
    public async Task ActivarCuentaSuspendida_DebeCambiarEstado()
    {
        // Arrange
        var repositorio = new FakeCuentaCorrienteRepository();
        var unitOfWork = new FakeUnitOfWork();

        var cuenta = new CuentaCorriente(
            clienteId: 1,
            limiteDeuda: 500000m);

        cuenta.Suspender();

        repositorio.ConfigurarCuenta(1, cuenta);

        var casoDeUso = new ActivarCuentaCorriente(
            repositorio,
            unitOfWork);

        // Act
        await casoDeUso.EjecutarAsync(1);

        // Assert
        Assert.Equal(
            EstadoCuentaCorriente.Activa,
            cuenta.Estado);

        Assert.True(unitOfWork.GuardarCambiosFueLlamado);
    }
}