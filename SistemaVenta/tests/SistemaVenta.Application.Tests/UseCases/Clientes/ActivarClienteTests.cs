
using SistemaVenta.Application.Tests.Fakes;
using SistemaVenta.Application.UseCases.Clientes;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;
using Xunit;

namespace SistemaVenta.Application.Tests.UseCases.Clientes
{
    public class ActivarClienteTests
    {
        [Fact]
        public async Task EjecutarAsync_ClienteInactivo_DeberiaActivarlo()
        {
            var repository = new FakeClienteRepository();

            var cliente = new ClienteFisico(
                "Juan",
                "Pérez",
                "40123456",
                "3811234567",
                "juan@email.com",
                "San Martín 123");

            cliente.Desactivar();

            repository.ConfigurarCliente(1, cliente);

            var unitOfWork = new FakeUnitOfWork();

            var casoDeUso = new ActivarCliente(
                repository,
                unitOfWork);

            await casoDeUso.EjecutarAsync(1);

            Assert.Equal(
                EstadoCliente.Activo,
                cliente.Estado);

            Assert.True(unitOfWork.GuardarCambiosFueLlamado);
        }

        [Fact]
        public async Task EjecutarAsync_ClienteInexistente_DeberiaLanzarExcepcion()
        {
            var repository = new FakeClienteRepository();
            var unitOfWork = new FakeUnitOfWork();

            var casoDeUso = new ActivarCliente(
                repository,
                unitOfWork);

            var excepcion = await Assert.ThrowsAsync<InvalidOperationException>(
                () => casoDeUso.EjecutarAsync(999));

            Assert.Equal(
                "El cliente no existe.",
                excepcion.Message);

            Assert.False(unitOfWork.GuardarCambiosFueLlamado);
        }
    }
}
