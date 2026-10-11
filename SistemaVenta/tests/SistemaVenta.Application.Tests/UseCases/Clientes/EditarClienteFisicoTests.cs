
using SistemaVenta.Application.Tests.Fakes;
using SistemaVenta.Application.UseCases.Clientes;
using SistemaVenta.Domain.Entities;
using Xunit;

namespace SistemaVenta.Application.Tests.UseCases.Clientes
{
    public class EditarClienteFisicoTests
    {
        [Fact]
        public async Task EjecutarAsync_MismoDni_DeberiaPermitirEdicion()
        {
            // Arrange
            var clienteRepository = new FakeClienteRepository();

            var cliente = new ClienteFisico(
                "Juan",
                "Pérez",
                "40123456",
                "3811234567",
                "juan@email.com",
                "San Martín 123");

            clienteRepository.ConfigurarCliente(1, cliente);

            var unitOfWork = new FakeUnitOfWork();

            var casoDeUso = new EditarClienteFisico(
                clienteRepository,
                unitOfWork);

            // Act
            await casoDeUso.EjecutarAsync(
                1,
                "Juan",
                "Pérez",
                "40123456",
                "3819999999",
                "juan@email.com",
                "Belgrano 456");

            // Assert
            Assert.Equal("40123456", cliente.Dni);
            Assert.Equal("3819999999", cliente.Telefono);
            Assert.Equal("Belgrano 456", cliente.Direccion);

            Assert.True(
                unitOfWork.GuardarCambiosFueLlamado);
        }

        [Fact]
        public async Task EjecutarAsync_DniDeOtroCliente_DeberiaRechazarEdicion()
        {
            // Arrange
            var clienteRepository = new FakeClienteRepository();

            var juan = new ClienteFisico(
                "Juan",
                "Pérez",
                "40123456",
                "3811234567",
                "juan@email.com",
                "San Martín 123");

            var pedro = new ClienteFisico(
                "Pedro",
                "Gómez",
                "38987654",
                "3819876543",
                "pedro@email.com",
                "Belgrano 456");

            clienteRepository.ConfigurarCliente(1, juan);
            clienteRepository.ConfigurarCliente(2, pedro);

            var unitOfWork = new FakeUnitOfWork();

            var casoDeUso = new EditarClienteFisico(
                clienteRepository,
                unitOfWork);

            // Act
            var excepcion = await Assert.ThrowsAsync<InvalidOperationException>(
                () => casoDeUso.EjecutarAsync(
                    1,
                    "Juan",
                    "Pérez",
                    "38987654",
                    "3819999999",
                    "juan@email.com",
                    "Nueva dirección"));

            // Assert
            Assert.Equal(
                "Ya existe otro cliente con ese DNI.",
                excepcion.Message);

            Assert.Equal("40123456", juan.Dni);
            Assert.Equal("3811234567", juan.Telefono);
            Assert.Equal("San Martín 123", juan.Direccion);

            Assert.False(
                unitOfWork.GuardarCambiosFueLlamado);
        }

        [Fact]
        public async Task EjecutarAsync_CuitDeEmpresa_DeberiaRechazarEdicion()
        {
            var repository = new FakeClienteRepository();

            var fisico = new ClienteFisico(
                "Juan", "Pérez", "40123456",
                "3811234567", "juan@email.com",
                "San Martín 123");

            var empresa = new ClienteEmpresa(
                "Tecnología del Norte S.R.L.",
                "30712345678",
                "3814567890",
                "ventas@empresa.com",
                "San Martín 500");

            repository.ConfigurarCliente(1, fisico);
            repository.ConfigurarCliente(2, empresa);

            var unitOfWork = new FakeUnitOfWork();
            var casoDeUso = new EditarClienteFisico(repository, unitOfWork);

            var excepcion = await Assert.ThrowsAsync<InvalidOperationException>(
                () => casoDeUso.EjecutarAsync(
                    1,
                    "Juan",
                    "Pérez",
                    "40123456",
                    "3819999999",
                    "juan@email.com",
                    "Nueva dirección",
                    "30712345678"));

            Assert.Equal(
                "Ya existe otro cliente con ese CUIT.",
                excepcion.Message);

            Assert.Null(fisico.Cuit);
            Assert.Equal("3811234567", fisico.Telefono);
            Assert.False(unitOfWork.GuardarCambiosFueLlamado);
        }

        [Fact]
        public async Task EjecutarAsync_EmailDeOtroCliente_DeberiaRechazarEdicion()
        {
            var repository = new FakeClienteRepository();

            var juan = new ClienteFisico(
                "Juan", "Pérez", "40123456",
                "3811234567", "juan@email.com",
                "San Martín 123");

            var pedro = new ClienteFisico(
                "Pedro", "Gómez", "38987654",
                "3819876543", "pedro@email.com",
                "Belgrano 456");

            repository.ConfigurarCliente(1, juan);
            repository.ConfigurarCliente(2, pedro);

            var unitOfWork = new FakeUnitOfWork();
            var casoDeUso = new EditarClienteFisico(repository, unitOfWork);

            var excepcion = await Assert.ThrowsAsync<InvalidOperationException>(
                () => casoDeUso.EjecutarAsync(
                    1,
                    "Juan",
                    "Pérez",
                    "40123456",
                    "3819999999",
                    "PEDRO@email.com",
                    "Nueva dirección"));

            Assert.Equal(
                "Ya existe otro cliente con ese correo electrónico.",
                excepcion.Message);

            Assert.Equal("juan@email.com", juan.Email);
            Assert.Equal("3811234567", juan.Telefono);
            Assert.False(unitOfWork.GuardarCambiosFueLlamado);
        }

        [Fact]
        public async Task EjecutarAsync_ClienteInexistente_DeberiaLanzarExcepcion()
        {
            var repository = new FakeClienteRepository();
            var unitOfWork = new FakeUnitOfWork();

            var casoDeUso = new EditarClienteFisico(repository, unitOfWork);

            var excepcion = await Assert.ThrowsAsync<InvalidOperationException>(
                () => casoDeUso.EjecutarAsync(
                    999,
                    "Juan",
                    "Pérez",
                    "40123456",
                    "3811234567",
                    "juan@email.com",
                    "San Martín 123"));

            Assert.Equal(
                "El cliente físico no existe.",
                excepcion.Message);

            Assert.False(unitOfWork.GuardarCambiosFueLlamado);
        }

    }
}
