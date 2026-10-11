using SistemaVenta.Application.Tests.Fakes;
using SistemaVenta.Application.UseCases.Clientes;
using SistemaVenta.Domain.Entities;
using Xunit;

namespace SistemaVenta.Application.Tests.UseCases.Clientes
{
    public class ListarClientesTests
    {
        [Fact]
        public async Task EjecutarAsync_ClientesExistentes_DeberiaListarlos()
        {
            var clienteRepository = new FakeClienteRepository();

            var fisico = ClienteTestFactory.ConId(
                new ClienteFisico(
                    "Juan",
                    "Pérez",
                    "40123456",
                    "3811234567",
                    "juan@email.com",
                    "San Martín 123"),
                1);

            var empresa = ClienteTestFactory.ConId(
                new ClienteEmpresa(
                    "Tecnología del Norte S.R.L.",
                    "30712345678",
                    "3814567890",
                    "ventas@empresa.com",
                    "San Martín 500"),
                2);

            clienteRepository.ConfigurarCliente(1, fisico);
            clienteRepository.ConfigurarCliente(2, empresa);

            var casoDeUso = new ListarClientes(clienteRepository);

            var resultado = await casoDeUso.EjecutarAsync();

            Assert.Equal(2, resultado.Count);

            var clienteFisico = Assert.Single(
                resultado,
                c => c.Id == 1);

            Assert.Equal("Físico", clienteFisico.Tipo);
            Assert.Equal("Juan Pérez", clienteFisico.Nombre);
            Assert.Equal("40123456", clienteFisico.Documento);

            var clienteEmpresa = Assert.Single(
                resultado,
                c => c.Id == 2);

            Assert.Equal("Empresa", clienteEmpresa.Tipo);
            Assert.Equal(
                "Tecnología del Norte S.R.L.",
                clienteEmpresa.Nombre);

            Assert.Equal(
                "30712345678",
                clienteEmpresa.Documento);
        }

        [Fact]
        public async Task EjecutarAsync_SinClientes_DeberiaDevolverListaVacia()
        {
            var clienteRepository = new FakeClienteRepository();

            var casoDeUso = new ListarClientes(clienteRepository);

            var resultado = await casoDeUso.EjecutarAsync();

            Assert.Empty(resultado);
        }
    }
}
