using SistemaVenta.Application.Tests.Fakes;
using SistemaVenta.Application.UseCases.Clientes;
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Application.Tests.UseCases.Clientes
{
    public class CrearClienteEmpresaTests
    {
        [Fact]
        public async Task EjecutarAsync_DatosValidos_DeberiaCrearEmpresa()
        {
            var clienteRepository = new FakeClienteRepository();
            var unitOfWork = new FakeUnitOfWork();

            var casoDeUso = new CrearClienteEmpresa(
                clienteRepository,
                unitOfWork);

            await casoDeUso.EjecutarAsync(
                "Tecnología del Norte S.R.L.",
                "30712345678",
                "3814567890",
                "ventas@empresa.com",
                "San Martín 500",
                "Carlos Gómez");

            var clientes =
                await clienteRepository.ObtenerTodosAsync();

            var empresa = Assert.IsType<ClienteEmpresa>(
                Assert.Single(clientes));

            Assert.Equal(
                "Tecnología del Norte S.R.L.",
                empresa.RazonSocial);

            Assert.Equal("30712345678", empresa.Cuit);
            Assert.Equal("Carlos Gómez", empresa.Contacto);

            Assert.True(
                unitOfWork.GuardarCambiosFueLlamado);
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

            clienteRepository.ConfigurarCliente(
                1,
                empresaExistente);

            var unitOfWork = new FakeUnitOfWork();

            var casoDeUso = new CrearClienteEmpresa(
                clienteRepository,
                unitOfWork);

            var excepcion = await Assert.ThrowsAsync<InvalidOperationException>(
                () => casoDeUso.EjecutarAsync(
                    "Informática del Sur S.A.",
                    "30712345678",
                    "3819876543",
                    "contacto@informatica.com",
                    "Belgrano 456"));

            Assert.Equal(
                "Ya existe un cliente con ese CUIT.",
                excepcion.Message);

            var clientes = await clienteRepository.ObtenerTodosAsync();

            Assert.Single(clientes);

            Assert.False(
                unitOfWork.GuardarCambiosFueLlamado);
        }
        [Fact]
        public async Task EjecutarAsync_EmailDuplicado_DeberiaLanzarExcepcion()
        {
            var empresaExistente = new ClienteEmpresa(
                "Tecnología del Norte S.R.L.",
                "30712345678",
                "3814567890",
                "ventas@empresa.com",
                "San Martín 500");

            var clienteRepository = new FakeClienteRepository();

            clienteRepository.ConfigurarCliente(
                1,
                empresaExistente);

            var unitOfWork = new FakeUnitOfWork();

            var casoDeUso = new CrearClienteEmpresa(
                clienteRepository,
                unitOfWork);

            var excepcion = await Assert.ThrowsAsync<InvalidOperationException>(
                () => casoDeUso.EjecutarAsync(
                    "Informática del Sur S.A.",
                    "30987654321",
                    "3819876543",
                    "VENTAS@empresa.com",
                    "Belgrano 456"));

            Assert.Equal(
                "Ya existe un cliente con ese correo electrónico.",
                excepcion.Message);

            var clientes = await clienteRepository.ObtenerTodosAsync();

            Assert.Single(clientes);

            Assert.False(
                unitOfWork.GuardarCambiosFueLlamado);
        }
    }
}
