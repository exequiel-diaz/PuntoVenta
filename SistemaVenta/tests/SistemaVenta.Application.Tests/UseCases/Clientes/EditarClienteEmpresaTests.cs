
using SistemaVenta.Application.Tests.Fakes;
using SistemaVenta.Application.UseCases.Clientes;
using SistemaVenta.Domain.Entities;
using Xunit;

namespace SistemaVenta.Application.Tests.UseCases.Clientes
{
    public class EditarClienteEmpresaTests
    {
        [Fact]
        public async Task EjecutarAsync_DatosValidos_DeberiaActualizarEmpresa()
        {
            var repository = new FakeClienteRepository();

            var empresa = new ClienteEmpresa(
                "Tecnología del Norte S.R.L.",
                "30712345678",
                "3814567890",
                "ventas@empresa.com",
                "San Martín 500");

            repository.ConfigurarCliente(1, empresa);

            var unitOfWork = new FakeUnitOfWork();

            var casoDeUso = new EditarClienteEmpresa(
                repository,
                unitOfWork);

            await casoDeUso.EjecutarAsync(
                1,
                "Tecnología Argentina S.A.",
                "30712345678",
                "3819999999",
                "contacto@empresa.com",
                "Belgrano 456",
                "Carlos Gómez");

            Assert.Equal(
                "Tecnología Argentina S.A.",
                empresa.RazonSocial);

            Assert.Equal("30712345678", empresa.Cuit);
            Assert.Equal("3819999999", empresa.Telefono);
            Assert.Equal("contacto@empresa.com", empresa.Email);
            Assert.Equal("Carlos Gómez", empresa.Contacto);

            Assert.True(unitOfWork.GuardarCambiosFueLlamado);
        }

        [Fact]
        public async Task EjecutarAsync_CuitDuplicado_DeberiaRechazarEdicion()
        {
            var repository = new FakeClienteRepository();

            var empresa = new ClienteEmpresa(
                "Tecnología del Norte S.R.L.",
                "30712345678",
                "3814567890",
                "ventas@empresa.com",
                "San Martín 500");

            var otraEmpresa = new ClienteEmpresa(
                "Informática del Sur S.A.",
                "30987654321",
                "3819876543",
                "info@informatica.com",
                "Belgrano 456");

            repository.ConfigurarCliente(1, empresa);
            repository.ConfigurarCliente(2, otraEmpresa);

            var unitOfWork = new FakeUnitOfWork();

            var casoDeUso = new EditarClienteEmpresa(
                repository,
                unitOfWork);

            var excepcion = await Assert.ThrowsAsync<InvalidOperationException>(
                () => casoDeUso.EjecutarAsync(
                    1,
                    "Tecnología Argentina S.A.",
                    "30987654321",
                    "3819999999",
                    "contacto@empresa.com",
                    "Nueva dirección"));

            Assert.Equal(
                "Ya existe otro cliente con ese CUIT.",
                excepcion.Message);

            Assert.Equal(
                "Tecnología del Norte S.R.L.",
                empresa.RazonSocial);

            Assert.Equal("30712345678", empresa.Cuit);

            Assert.False(unitOfWork.GuardarCambiosFueLlamado);
        }

        [Fact]
        public async Task EjecutarAsync_EmailDuplicado_DeberiaRechazarEdicion()
        {
            var repository = new FakeClienteRepository();

            var empresa = new ClienteEmpresa(
                "Tecnología del Norte S.R.L.",
                "30712345678",
                "3814567890",
                "ventas@empresa.com",
                "San Martín 500");

            var clienteFisico = new ClienteFisico(
                "Juan",
                "Pérez",
                "40123456",
                "3811234567",
                "juan@email.com",
                "Belgrano 123");

            repository.ConfigurarCliente(1, empresa);
            repository.ConfigurarCliente(2, clienteFisico);

            var unitOfWork = new FakeUnitOfWork();

            var casoDeUso = new EditarClienteEmpresa(
                repository,
                unitOfWork);

            var excepcion = await Assert.ThrowsAsync<InvalidOperationException>(
                () => casoDeUso.EjecutarAsync(
                    1,
                    "Tecnología Argentina S.A.",
                    "30712345678",
                    "3819999999",
                    "JUAN@email.com",
                    "Nueva dirección"));

            Assert.Equal(
                "Ya existe otro cliente con ese correo electrónico.",
                excepcion.Message);

            Assert.Equal("ventas@empresa.com", empresa.Email);
            Assert.Equal(
                "Tecnología del Norte S.R.L.",
                empresa.RazonSocial);

            Assert.False(unitOfWork.GuardarCambiosFueLlamado);
        }

        [Fact]
        public async Task EjecutarAsync_EmpresaInexistente_DeberiaLanzarExcepcion()
        {
            var repository = new FakeClienteRepository();
            var unitOfWork = new FakeUnitOfWork();

            var casoDeUso = new EditarClienteEmpresa(
                repository,
                unitOfWork);

            var excepcion = await Assert.ThrowsAsync<InvalidOperationException>(
                () => casoDeUso.EjecutarAsync(
                    999,
                    "Tecnología Argentina S.A.",
                    "30712345678",
                    "3819999999",
                    "ventas@empresa.com",
                    "San Martín 500"));

            Assert.Equal(
                "La empresa no existe.",
                excepcion.Message);

            Assert.False(unitOfWork.GuardarCambiosFueLlamado);
        }

        [Fact]
        public async Task EjecutarAsync_IdDeClienteFisico_DeberiaRechazarEdicion()
        {
            var repository = new FakeClienteRepository();

            var fisico = new ClienteFisico(
                "Juan",
                "Pérez",
                "40123456",
                "3811234567",
                "juan@email.com",
                "San Martín 123");

            repository.ConfigurarCliente(1, fisico);

            var unitOfWork = new FakeUnitOfWork();

            var casoDeUso = new EditarClienteEmpresa(
                repository,
                unitOfWork);

            var excepcion = await Assert.ThrowsAsync<InvalidOperationException>(
                () => casoDeUso.EjecutarAsync(
                    1,
                    "Tecnología Argentina S.A.",
                    "30712345678",
                    "3819999999",
                    "ventas@empresa.com",
                    "San Martín 500"));

            Assert.Equal(
                "La empresa no existe.",
                excepcion.Message);

            Assert.Equal("Juan", fisico.Nombre);
            Assert.False(unitOfWork.GuardarCambiosFueLlamado);
        }

    }
}
