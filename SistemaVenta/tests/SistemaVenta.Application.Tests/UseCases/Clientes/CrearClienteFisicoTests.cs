using SistemaVenta.Application.Tests.Fakes;
using SistemaVenta.Application.UseCases.Clientes;
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Application.Tests.UseCases.Clientes
{
    public class CrearClienteFisicoTests
    {
        [Fact]
        public async Task EjecutarAsync_DatosValidos_DeberiaCrearCliente()
        {
            var clienteRepository = new FakeClienteRepository();
            var unitOfWork = new FakeUnitOfWork();

            var casoDeUso = new CrearClienteFisico(
                clienteRepository,
                unitOfWork);

            await casoDeUso.EjecutarAsync(
                "Juan",
                "Pérez",
                "40123456",
                "3811234567",
                "juan@email.com",
                "San Martín 123");

            var clientes =
                await clienteRepository.ObtenerTodosAsync();

            Assert.Single(clientes);

            var cliente = Assert.IsType<ClienteFisico>(
                clientes[0]);

            Assert.Equal("Juan", cliente.Nombre);
            Assert.Equal("Pérez", cliente.Apellido);
            Assert.Equal("40123456", cliente.Dni);

            Assert.True(
                unitOfWork.GuardarCambiosFueLlamado);
        }

        [Fact]
        public async Task EjecutarAsync_DniDuplicado_DeberiaLanzarExcepcion()
        {
            var clienteExistente = new ClienteFisico(
                "Juan",
                "Pérez",
                "40123456",
                "3811234567",
                "juan@email.com",
                "San Martín 123");

            var clienteRepository = new FakeClienteRepository();

            clienteRepository.ConfigurarCliente(
                1,
                clienteExistente);

            var unitOfWork = new FakeUnitOfWork();

            var casoDeUso = new CrearClienteFisico(
                clienteRepository,
                unitOfWork);

            var excepcion = await Assert.ThrowsAsync<InvalidOperationException>(
                () => casoDeUso.EjecutarAsync(
                    "Pedro",
                    "Gómez",
                    "40123456",
                    "3819876543",
                    "pedro@email.com",
                    "Belgrano 456"));

            Assert.Equal(
                "Ya existe un cliente con ese DNI.",
                excepcion.Message);

            var clientes =
                await clienteRepository.ObtenerTodosAsync();

            Assert.Single(clientes);

            Assert.False(
                unitOfWork.GuardarCambiosFueLlamado);
        }

        [Fact]
        public async Task EjecutarAsync_EmailDuplicado_DeberiaLanzarExcepcion()
        {
            var clienteExistente = new ClienteFisico(
                "Juan",
                "Pérez",
                "40123456",
                "3811234567",
                "juan@email.com",
                "San Martín 123");

            var clienteRepository = new FakeClienteRepository();

            clienteRepository.ConfigurarCliente(1, clienteExistente);

            var unitOfWork = new FakeUnitOfWork();

            var casoDeUso = new CrearClienteFisico(
                clienteRepository,
                unitOfWork);

            var excepcion = await Assert.ThrowsAsync<InvalidOperationException>(
                () => casoDeUso.EjecutarAsync(
                    "Pedro",
                    "Gómez",
                    "38987654",
                    "3819876543",
                    "JUAN@email.com",
                    "Belgrano 456"));

            Assert.Equal(
                "Ya existe un cliente con ese correo electrónico.",
                excepcion.Message);

            var clientes = await clienteRepository.ObtenerTodosAsync();

            Assert.Single(clientes);
            Assert.False(unitOfWork.GuardarCambiosFueLlamado);
        }


        [Fact]
        public async Task EjecutarAsync_CuitDuplicado_DeberiaLanzarExcepcion()
        {
            var empresaExistente = new ClienteEmpresa(
                "Tecnología del Norte S.R.L.",
                "30712345678",
                "3814567890",
                "ventas@empresa.com",
                "San Martín 500");

            var clienteRepository = new FakeClienteRepository();

            clienteRepository.ConfigurarCliente(1, empresaExistente);

            var unitOfWork = new FakeUnitOfWork();

            var casoDeUso = new CrearClienteFisico(
                clienteRepository,
                unitOfWork);

            var excepcion = await Assert.ThrowsAsync<InvalidOperationException>(
                () => casoDeUso.EjecutarAsync(
                    "Pedro",
                    "Gómez",
                    "38987654",
                    "3819876543",
                    "pedro@email.com",
                    "Belgrano 456",
                    "30712345678"));

            Assert.Equal(
                "Ya existe un cliente con ese CUIT.",
                excepcion.Message);

            var clientes = await clienteRepository.ObtenerTodosAsync();

            Assert.Single(clientes);
            Assert.False(unitOfWork.GuardarCambiosFueLlamado);
        }


        [Fact]
        public async Task EjecutarAsync_SinCuit_DeberiaCrearCliente()
        {
            var clienteRepository = new FakeClienteRepository();
            var unitOfWork = new FakeUnitOfWork();

            var casoDeUso = new CrearClienteFisico(
                clienteRepository,
                unitOfWork);

            await casoDeUso.EjecutarAsync(
                "María",
                "López",
                "38987654",
                "3819876543",
                "maria@email.com",
                "Belgrano 456");

            var clientes = await clienteRepository.ObtenerTodosAsync();

            var cliente = Assert.IsType<ClienteFisico>(
                Assert.Single(clientes));

            Assert.Null(cliente.Cuit);
            Assert.True(unitOfWork.GuardarCambiosFueLlamado);
        }

    }
}
