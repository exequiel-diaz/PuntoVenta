
using SistemaVenta.Application.Tests.Fakes;
using SistemaVenta.Application.UseCases.CuentasCorrientes;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Application.Tests.CuentasCorrientes;

public class RegistrarCreditoCuentaCorrienteTests
{
    [Fact]
    public async Task RegistrarCredito_Valido_DisminuyeSaldoYGuardaCambios()
    {
        // Arrange
        var repositorio = new FakeCuentaCorrienteRepository();
        var unitOfWork = new FakeUnitOfWork();

        var cuenta = new CuentaCorriente(
            clienteId: 1,
            limiteDeuda: 100000m);

        cuenta.RegistrarDebito(50000m, "Compra inicial");

        repositorio.ConfigurarCuenta(1, cuenta);

        var casoDeUso = new RegistrarCreditoCuentaCorriente(
            repositorio,
            unitOfWork);

        // Act
        await casoDeUso.EjecutarAsync(
            cuentaId: 1,
            importe: 20000m,
            descripcion: "Pago parcial");

        // Assert
        Assert.Equal(30000m, cuenta.SaldoActual);
        Assert.Equal(2, cuenta.Movimientos.Count);

        var movimiento = cuenta.Movimientos.Last();

        Assert.Equal(TipoMovimientoCuenta.Credito, movimiento.Tipo);
        Assert.Equal(20000m, movimiento.Importe);

        Assert.True(unitOfWork.GuardarCambiosFueLlamado);
    }

    [Fact]
    public async Task RegistrarCredito_CuentaInexistente_LanzaExcepcion()
    {
        // Arrange
        var repositorio = new FakeCuentaCorrienteRepository();
        var unitOfWork = new FakeUnitOfWork();

        var casoDeUso = new RegistrarCreditoCuentaCorriente(
            repositorio,
            unitOfWork);

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => casoDeUso.EjecutarAsync(
                cuentaId: 999,
                importe: 10000m,
                descripcion: "Pago de prueba"));

        // Assert
        Assert.False(unitOfWork.GuardarCambiosFueLlamado);
    }

    [Fact]
    public async Task RegistrarCredito_SuperaDeuda_LanzaExcepcion()
    {
        // Arrange
        var repositorio = new FakeCuentaCorrienteRepository();
        var unitOfWork = new FakeUnitOfWork();

        var cuenta = new CuentaCorriente(
            clienteId: 1,
            limiteDeuda: 100000m);

        cuenta.RegistrarDebito(10000m, "Compra inicial");

        repositorio.ConfigurarCuenta(1, cuenta);

        var casoDeUso = new RegistrarCreditoCuentaCorriente(
            repositorio,
            unitOfWork);

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => casoDeUso.EjecutarAsync(
                cuentaId: 1,
                importe: 15000m,
                descripcion: "Pago superior a la deuda"));

        // Assert
        Assert.Equal(10000m, cuenta.SaldoActual);
        Assert.Single(cuenta.Movimientos);
        Assert.False(unitOfWork.GuardarCambiosFueLlamado);
    }

    [Fact]
    public async Task RegistrarCredito_CuentaSuspendida_PermitePago()
    {
        // Arrange
        var repositorio = new FakeCuentaCorrienteRepository();
        var unitOfWork = new FakeUnitOfWork();

        var cuenta = new CuentaCorriente(
            clienteId: 1,
            limiteDeuda: 100000m);

        cuenta.RegistrarDebito(30000m, "Compra inicial");
        cuenta.Suspender();

        repositorio.ConfigurarCuenta(1, cuenta);

        var casoDeUso = new RegistrarCreditoCuentaCorriente(
            repositorio,
            unitOfWork);

        // Act
        await casoDeUso.EjecutarAsync(
            cuentaId: 1,
            importe: 10000m,
            descripcion: "Pago con cuenta suspendida");

        // Assert
        Assert.Equal(20000m, cuenta.SaldoActual);
        Assert.Equal(2, cuenta.Movimientos.Count);
        Assert.Equal(
            EstadoCuentaCorriente.Suspendida,
            cuenta.Estado);

        Assert.True(unitOfWork.GuardarCambiosFueLlamado);
    }

    [Fact]
    public async Task RegistrarCredito_CuentaCerrada_LanzaExcepcion()
    {
        // Arrange
        var repositorio = new FakeCuentaCorrienteRepository();
        var unitOfWork = new FakeUnitOfWork();

        var cuenta = new CuentaCorriente(
            clienteId: 1,
            limiteDeuda: 100000m);

        cuenta.Cerrar();

        repositorio.ConfigurarCuenta(1, cuenta);

        var casoDeUso = new RegistrarCreditoCuentaCorriente(
            repositorio,
            unitOfWork);

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => casoDeUso.EjecutarAsync(
                cuentaId: 1,
                importe: 10000m,
                descripcion: "Pago con cuenta cerrada"));

        // Assert
        Assert.Equal(0m, cuenta.SaldoActual);
        Assert.Empty(cuenta.Movimientos);
        Assert.False(unitOfWork.GuardarCambiosFueLlamado);
    }
}
