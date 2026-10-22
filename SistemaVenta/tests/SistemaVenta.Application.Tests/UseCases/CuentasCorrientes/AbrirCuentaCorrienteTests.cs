
using SistemaVenta.Application.Tests.Fakes;
using SistemaVenta.Application.UseCases.CuentasCorrientes;
using SistemaVenta.Domain.Entities;
using Xunit;

namespace SistemaVenta.Application.Tests.UseCases.CuentasCorrientes;

public class AbrirCuentaCorrienteTests
{
    private static ClienteFisico CrearClienteValido()
    {
        return new ClienteFisico(
            nombre: "Juan",
            apellido: "Pérez",
            dni: "12345678",
            telefono: "3811111111",
            email: "juan@ejemplo.com",
            direccion: "Tucumán"
        );
    }

    [Fact]
    public async Task AbrirCuenta_ClienteActivo_DebeCrearCuenta()
    {
        // Arrange
        var clientes = new FakeClienteRepository();
        var cuentas = new FakeCuentaCorrienteRepository();
        var unitOfWork = new FakeUnitOfWork();

        clientes.ConfigurarCliente(1, CrearClienteValido());

        var casoDeUso = new AbrirCuentaCorriente(
            cuentas,
            clientes,
            unitOfWork);

        // Act
        await casoDeUso.EjecutarAsync(
            clienteId: 1,
            limiteDeuda: 500000m);

        // Assert
        var cuenta = await cuentas.ObtenerPorClienteIdAsync(1);

        Assert.NotNull(cuenta);
        Assert.Equal(500000m, cuenta.LimiteDeuda);
        Assert.Equal(0m, cuenta.SaldoActual);
        Assert.True(unitOfWork.GuardarCambiosFueLlamado);
    }

    [Fact]
    public async Task AbrirCuenta_ClienteInexistente_DebeFallar()
    {
        var clientes = new FakeClienteRepository();
        var cuentas = new FakeCuentaCorrienteRepository();
        var unitOfWork = new FakeUnitOfWork();

        var casoDeUso = new AbrirCuentaCorriente(
            cuentas,
            clientes,
            unitOfWork);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => casoDeUso.EjecutarAsync(999, 500000m));

        Assert.False(unitOfWork.GuardarCambiosFueLlamado);
    }

    [Fact]
    public async Task AbrirCuenta_ClienteInactivo_DebeFallar()
    {
        var clientes = new FakeClienteRepository();
        var cuentas = new FakeCuentaCorrienteRepository();
        var unitOfWork = new FakeUnitOfWork();

        var cliente = CrearClienteValido();
        cliente.Desactivar();

        clientes.ConfigurarCliente(1, cliente);

        var casoDeUso = new AbrirCuentaCorriente(
            cuentas,
            clientes,
            unitOfWork);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => casoDeUso.EjecutarAsync(1, 500000m));

        Assert.False(unitOfWork.GuardarCambiosFueLlamado);
    }

    [Fact]
    public async Task AbrirCuenta_ClienteConCuentaExistente_DebeFallar()
    {
        var clientes = new FakeClienteRepository();
        var cuentas = new FakeCuentaCorrienteRepository();
        var unitOfWork = new FakeUnitOfWork();

        clientes.ConfigurarCliente(1, CrearClienteValido());

        cuentas.ConfigurarCuenta(
            1,
            new CuentaCorriente(1, 300000m));

        var casoDeUso = new AbrirCuentaCorriente(
            cuentas,
            clientes,
            unitOfWork);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => casoDeUso.EjecutarAsync(1, 500000m));

        Assert.False(unitOfWork.GuardarCambiosFueLlamado);
    }

    [Fact]
    public async Task AbrirCuenta_LimiteNegativo_DebeFallar()
    {
        var clientes = new FakeClienteRepository();
        var cuentas = new FakeCuentaCorrienteRepository();
        var unitOfWork = new FakeUnitOfWork();

        clientes.ConfigurarCliente(1, CrearClienteValido());

        var casoDeUso = new AbrirCuentaCorriente(
            cuentas,
            clientes,
            unitOfWork);

        await Assert.ThrowsAsync<ArgumentException>(
            () => casoDeUso.EjecutarAsync(1, -1000m));

        Assert.False(unitOfWork.GuardarCambiosFueLlamado);
    }
}
