
using SistemaVenta.Application.Tests.Fakes;
using SistemaVenta.Application.UseCases.CuentasCorrientes;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Application.Tests.CuentasCorrientes;

public class RegistrarDebitoCuentaCorrienteTests
{
    [Fact]
    public async Task RegistrarDebito_Valido_AumentaSaldoYGuardaCambios()
    {
        // Arrange
        var repositorio = new FakeCuentaCorrienteRepository();
        var unitOfWork = new FakeUnitOfWork();

        var cuenta = new CuentaCorriente(
            clienteId: 1,
            limiteDeuda: 100000m);

        repositorio.ConfigurarCuenta(1, cuenta);

        var casoDeUso = new RegistrarDebitoCuentaCorriente(
            repositorio,
            unitOfWork);

        // Act
        await casoDeUso.EjecutarAsync(
            cuentaId: 1,
            importe: 20000m,
            descripcion: "Compra de prueba");

        // Assert
        Assert.Equal(20000m, cuenta.SaldoActual);
        Assert.Single(cuenta.Movimientos);

        var movimiento = cuenta.Movimientos.Single();

        Assert.Equal(TipoMovimientoCuenta.Debito, movimiento.Tipo);
        Assert.Equal(20000m, movimiento.Importe);

        Assert.True(unitOfWork.GuardarCambiosFueLlamado);
    }

    [Fact]
    public async Task RegistrarDebito_CuentaInexistente_LanzaExcepcion()
    {
        // Arrange
        var repositorio = new FakeCuentaCorrienteRepository();
        var unitOfWork = new FakeUnitOfWork();

        var casoDeUso = new RegistrarDebitoCuentaCorriente(
            repositorio,
            unitOfWork);

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => casoDeUso.EjecutarAsync(
                cuentaId: 999,
                importe: 20000m,
                descripcion: "Compra de prueba"));

        // Assert
        Assert.False(unitOfWork.GuardarCambiosFueLlamado);
    }

    [Fact]
    public async Task RegistrarDebito_SuperaLimite_LanzaExcepcion()
    {
        // Arrange
        var repositorio = new FakeCuentaCorrienteRepository();
        var unitOfWork = new FakeUnitOfWork();

        var cuenta = new CuentaCorriente(
            clienteId: 1,
            limiteDeuda: 10000m);

        repositorio.ConfigurarCuenta(1, cuenta);

        var casoDeUso = new RegistrarDebitoCuentaCorriente(
            repositorio,
            unitOfWork);

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => casoDeUso.EjecutarAsync(
                cuentaId: 1,
                importe: 15000m,
                descripcion: "Compra que supera el límite"));

        // Assert
        Assert.Equal(0m, cuenta.SaldoActual);
        Assert.Empty(cuenta.Movimientos);
        Assert.False(unitOfWork.GuardarCambiosFueLlamado);
    }

    [Fact]
    public async Task RegistrarDebito_CuentaSuspendida_LanzaExcepcion()
    {
        // Arrange
        var repositorio = new FakeCuentaCorrienteRepository();
        var unitOfWork = new FakeUnitOfWork();

        var cuenta = new CuentaCorriente(
            clienteId: 1,
            limiteDeuda: 100000m);

        cuenta.Suspender();

        repositorio.ConfigurarCuenta(1, cuenta);

        var casoDeUso = new RegistrarDebitoCuentaCorriente(
            repositorio,
            unitOfWork);

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => casoDeUso.EjecutarAsync(
                cuentaId: 1,
                importe: 20000m,
                descripcion: "Compra con cuenta suspendida"));

        // Assert
        Assert.Equal(0m, cuenta.SaldoActual);
        Assert.Empty(cuenta.Movimientos);
        Assert.False(unitOfWork.GuardarCambiosFueLlamado);
    }
}
