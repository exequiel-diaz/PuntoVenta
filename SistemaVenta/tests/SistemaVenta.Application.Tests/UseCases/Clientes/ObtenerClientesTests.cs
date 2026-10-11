
using SistemaVenta.Application.Tests.Fakes;
using SistemaVenta.Application.UseCases.Clientes;
using SistemaVenta.Domain.Entities;
using Xunit;

namespace SistemaVenta.Application.Tests.UseCases.Clientes
{
    public class ObtenerClienteTests
    {
        [Fact]
        public async Task EjecutarAsync_ClienteFisicoExistente_DeberiaDevolverDetalle()
        {
            // Arrange
            var clienteRepository = new FakeClienteRepository();

            var cliente = ClienteTestFactory.ConId(
                new ClienteFisico(
                    "Juan",
                    "Pérez",
                    "40123456",
                    "3811234567",
                    "juan@email.com",
                    "San Martín 123",
                    "20401234567"),
                1);

            clienteRepository.ConfigurarCliente(1, cliente);

            var casoDeUso = new ObtenerCliente(clienteRepository);

            // Act
            var resultado = await casoDeUso.EjecutarAsync(1);

            // Assert
            Assert.Equal(1, resultado.Id);
            Assert.Equal("Físico", resultado.Tipo);
            Assert.Equal("Juan", resultado.Nombre);
            Assert.Equal("Pérez", resultado.Apellido);
            Assert.Equal("40123456", resultado.Dni);
            Assert.Equal("20401234567", resultado.Cuit);
            Assert.Equal("juan@email.com", resultado.Email);
            Assert.Null(resultado.RazonSocial);
        }

        [Fact]
        public async Task EjecutarAsync_ClienteEmpresaExistente_DeberiaDevolverDetalle()
        {
            // Arrange
            var clienteRepository = new FakeClienteRepository();

            var empresa = ClienteTestFactory.ConId(
                new ClienteEmpresa(
                    "Tecnología del Norte S.R.L.",
                    "30712345678",
                    "3814567890",
                    "ventas@empresa.com",
                    "San Martín 500",
                    "Carlos Gómez"),
                2);

            clienteRepository.ConfigurarCliente(2, empresa);

            var casoDeUso = new ObtenerCliente(clienteRepository);

            // Act
            var resultado = await casoDeUso.EjecutarAsync(2);

            // Assert
            Assert.Equal(2, resultado.Id);
            Assert.Equal("Empresa", resultado.Tipo);
            Assert.Equal(
                "Tecnología del Norte S.R.L.",
                resultado.RazonSocial);

            Assert.Equal("30712345678", resultado.Cuit);
            Assert.Equal("Carlos Gómez", resultado.Contacto);
            Assert.Null(resultado.Dni);
            Assert.Null(resultado.Nombre);
        }

        [Fact]
        public async Task EjecutarAsync_ClienteInexistente_DeberiaLanzarExcepcion()
        {
            // Arrange
            var clienteRepository = new FakeClienteRepository();

            var casoDeUso = new ObtenerCliente(clienteRepository);

            // Act
            var excepcion = await Assert.ThrowsAsync<InvalidOperationException>(
                () => casoDeUso.EjecutarAsync(999));

            // Assert
            Assert.Equal(
                "El cliente no existe.",
                excepcion.Message);
        }
    }
}
