using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Domain.Tests.Entities
{
    public class ClienteEmpresaTests
    {
        [Fact]
        public void Constructor_DatosValidos_DeberiaCrearEmpresaActiva()
        {
            var cliente = new ClienteEmpresa(
                "Tecnología del Norte S.R.L.",
                "30712345678",
                "3814567890",
                "ventas@tecnologiadelnorte.com",
                "San Martín 500",
                "Carlos Gómez",
                "Cliente mayorista");

            Assert.Equal(
                "Tecnología del Norte S.R.L.",
                cliente.RazonSocial);

            Assert.Equal(
                "30712345678",
                cliente.Cuit);

            Assert.Equal(
                "3814567890",
                cliente.Telefono);

            Assert.Equal(
                "ventas@tecnologiadelnorte.com",
                cliente.Email);

            Assert.Equal(
                "San Martín 500",
                cliente.Direccion);

            Assert.Equal(
                "Carlos Gómez",
                cliente.Contacto);

            Assert.Equal(
                "Cliente mayorista",
                cliente.Observaciones);

            Assert.Equal(
                EstadoCliente.Activo,
                cliente.Estado);

            Assert.True(cliente.EstaActivo());
        }

        [Fact]
        public void Constructor_RazonSocialVacia_DeberiaLanzarExcepcion()
        {
            Assert.Throws<ArgumentException>(
                () => new ClienteEmpresa(
                    "",
                    "30712345678",
                    "3814567890",
                    "ventas@empresa.com",
                    "San Martín 500"));
        }

        [Fact]
        public void Constructor_CuitVacio_DeberiaLanzarExcepcion()
        {
            Assert.Throws<ArgumentException>(
                () => new ClienteEmpresa(
                    "Tecnología del Norte S.R.L.",
                    "",
                    "3814567890",
                    "ventas@empresa.com",
                    "San Martín 500"));
        }

        [Fact]
        public void Desactivar_ClienteEmpresa_DeberiaQuedarInactivo()
        {
            var cliente = new ClienteEmpresa(
                "Tecnología del Norte S.R.L.",
                "30712345678",
                "3814567890",
                "ventas@empresa.com",
                "San Martín 500");

            cliente.Desactivar();

            Assert.Equal(
                EstadoCliente.Inactivo,
                cliente.Estado);

            Assert.False(cliente.EstaActivo());
        }

        [Fact]
        public void ActualizarDatosEmpresa_DatosValidos_DeberiaActualizarEmpresa()
        {
            var empresa = new ClienteEmpresa(
                "Tecnología del Norte S.R.L.",
                "30712345678",
                "3814567890",
                "ventas@empresa.com",
                "San Martín 500");

            empresa.ActualizarDatosEmpresa(
                "Tecnología Argentina S.A.",
                "30987654321",
                "Carlos Gómez");

            Assert.Equal(
                "Tecnología Argentina S.A.",
                empresa.RazonSocial);

            Assert.Equal("30987654321", empresa.Cuit);
            Assert.Equal("Carlos Gómez", empresa.Contacto);
        }

        [Fact]
        public void ActualizarDatosEmpresa_CuitVacio_DeberiaConservarDatos()
        {
            var empresa = new ClienteEmpresa(
                "Tecnología del Norte S.R.L.",
                "30712345678",
                "3814567890",
                "ventas@empresa.com",
                "San Martín 500");

            Assert.Throws<ArgumentException>(() =>
                empresa.ActualizarDatosEmpresa(
                    "Nueva Empresa S.A.",
                    "",
                    "Carlos Gómez"));

            Assert.Equal(
                "Tecnología del Norte S.R.L.",
                empresa.RazonSocial);

            Assert.Equal("30712345678", empresa.Cuit);
        }

    }
}
