
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Domain.Tests.Entities
{
    public class ClienteFisicoTests
    {
        [Fact]
        public void Constructor_DatosValidos_DeberiaCrearClienteActivo()
        {
            var cliente = new ClienteFisico(
                "Juan",
                "Pérez",
                "40123456",
                "3811234567",
                "juan@email.com",
                "San Martín 123",
                "20401234567",
                new DateTime(2000, 5, 10),
                "Cliente habitual");

            Assert.Equal("Juan", cliente.Nombre);
            Assert.Equal("Pérez", cliente.Apellido);
            Assert.Equal("40123456", cliente.Dni);
            Assert.Equal("3811234567", cliente.Telefono);
            Assert.Equal("juan@email.com", cliente.Email);
            Assert.Equal("San Martín 123", cliente.Direccion);
            Assert.Equal("20401234567", cliente.Cuit);
            Assert.Equal(
                new DateTime(2000, 5, 10),
                cliente.FechaNacimiento);

            Assert.Equal(
                EstadoCliente.Activo,
                cliente.Estado);

            Assert.True(cliente.EstaActivo());
        }

        [Fact]
        public void Constructor_NombreVacio_DeberiaLanzarExcepcion()
        {
            Assert.Throws<ArgumentException>(
                () => new ClienteFisico(
                    "",
                    "Pérez",
                    "40123456",
                    "3811234567",
                    "juan@email.com",
                    "San Martín 123"));
        }

        [Fact]
        public void Constructor_DniVacio_DeberiaLanzarExcepcion()
        {
            Assert.Throws<ArgumentException>(
                () => new ClienteFisico(
                    "Juan",
                    "Pérez",
                    "",
                    "3811234567",
                    "juan@email.com",
                    "San Martín 123"));
        }

        [Fact]
        public void Desactivar_ClienteActivo_DeberiaQuedarInactivo()
        {
            var cliente = new ClienteFisico(
                "Juan",
                "Pérez",
                "40123456",
                "3811234567",
                "juan@email.com",
                "San Martín 123");

            cliente.Desactivar();

            Assert.Equal(
                EstadoCliente.Inactivo,
                cliente.Estado);

            Assert.False(cliente.EstaActivo());
        }

        [Fact]
        public void ActualizarDatosPersonales_DatosValidos_DeberiaActualizarCliente()
        {
            // Arrange
            var cliente = new ClienteFisico(
                "Juan",
                "Pérez",
                "40123456",
                "3811234567",
                "juan@email.com",
                "San Martín 123");

            // Act
            cliente.ActualizarDatosPersonales(
                "Pedro",
                "Gómez",
                "38987654",
                "20389876543",
                new DateTime(1995, 5, 10));

            // Assert
            Assert.Equal("Pedro", cliente.Nombre);
            Assert.Equal("Gómez", cliente.Apellido);
            Assert.Equal("38987654", cliente.Dni);
            Assert.Equal("20389876543", cliente.Cuit);
            Assert.Equal(
                new DateTime(1995, 5, 10),
                cliente.FechaNacimiento);
        }

        [Fact]
        public void ActualizarDatosPersonales_DniVacio_DeberiaLanzarExcepcion()
        {
            // Arrange
            var cliente = new ClienteFisico(
                "Juan",
                "Pérez",
                "40123456",
                "3811234567",
                "juan@email.com",
                "San Martín 123");

            // Act
            Assert.Throws<ArgumentException>(() =>
                cliente.ActualizarDatosPersonales(
                    "Pedro",
                    "Gómez",
                    "",
                    null,
                    null));

            // Assert
            Assert.Equal("Juan", cliente.Nombre);
            Assert.Equal("Pérez", cliente.Apellido);
            Assert.Equal("40123456", cliente.Dni);
        }

        [Fact]
        public void ActualizarDatosContacto_DatosValidos_DeberiaActualizarContacto()
        {
            var cliente = new ClienteFisico(
                "Juan",
                "Pérez",
                "40123456",
                "3811234567",
                "juan@email.com",
                "San Martín 123");

            cliente.ActualizarDatosContacto(
                "3819999999",
                "nuevo@email.com",
                "Belgrano 456",
                "Cliente habitual");

            Assert.Equal("3819999999", cliente.Telefono);
            Assert.Equal("nuevo@email.com", cliente.Email);
            Assert.Equal("Belgrano 456", cliente.Direccion);
            Assert.Equal("Cliente habitual", cliente.Observaciones);
        }

        [Fact]
        public void ActualizarDatosContacto_EmailVacio_DeberiaConservarDatos()
        {
            var cliente = new ClienteFisico(
                "Juan",
                "Pérez",
                "40123456",
                "3811234567",
                "juan@email.com",
                "San Martín 123");

            Assert.Throws<ArgumentException>(() =>
                cliente.ActualizarDatosContacto(
                    "3819999999",
                    "",
                    "Belgrano 456",
                    null));

            Assert.Equal("3811234567", cliente.Telefono);
            Assert.Equal("juan@email.com", cliente.Email);
            Assert.Equal("San Martín 123", cliente.Direccion);
        }

    }
}
