using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;
using Xunit;

namespace SistemaVenta.Domain.Tests.Entities
{
    public class CuentaCorrienteTests
    {
        [Fact]
        public void CrearCuentaCorriente_DebeIniciarActivaYConSaldoCero()
        {
            // Arrange y Act
            var cuenta = new CuentaCorriente(
                clienteId: 1,
                limiteDeuda: 500000m);

            // Assert
            Assert.Equal(1, cuenta.ClienteId);
            Assert.Equal(500000m, cuenta.LimiteDeuda);
            Assert.Equal(EstadoCuentaCorriente.Activa, cuenta.Estado);
            Assert.Equal(0m, cuenta.SaldoActual);
            Assert.Empty(cuenta.Movimientos);
        }

        [Fact]
        public void RegistrarDebito_DebeAumentarSaldo()
        {
            // Arrange
            var cuenta = new CuentaCorriente(
                clienteId: 1,
                limiteDeuda: 500000m);

            // Act
            cuenta.RegistrarDebito(
                150000m,
                "Venta a cuenta corriente");

            // Assert
            Assert.Equal(150000m, cuenta.SaldoActual);

            var movimiento = Assert.Single(cuenta.Movimientos);

            Assert.Equal(TipoMovimientoCuenta.Debito, movimiento.Tipo);
            Assert.Equal(150000m, movimiento.Importe);
        }

        [Fact]
        public void RegistrarDebito_SiSuperaLimite_DebeFallar()
        {
            // Arrange
            var cuenta = new CuentaCorriente(
                clienteId: 1,
                limiteDeuda: 200000m);

            cuenta.RegistrarDebito(
                150000m,
                "Primera venta");

            // Act
            Assert.Throws<InvalidOperationException>(() =>
                cuenta.RegistrarDebito(
                    100000m,
                    "Segunda venta"));

            // Assert
            Assert.Equal(150000m, cuenta.SaldoActual);
            Assert.Single(cuenta.Movimientos);
        }

        [Fact]
        public void RegistrarCredito_DebeReducirSaldo()
        {
            // Arrange
            var cuenta = new CuentaCorriente(
                clienteId: 1,
                limiteDeuda: 500000m);

            cuenta.RegistrarDebito(
                200000m,
                "Venta");

            // Act
            cuenta.RegistrarCredito(
                50000m,
                "Pago parcial");

            // Assert
            Assert.Equal(150000m, cuenta.SaldoActual);
            Assert.Equal(2, cuenta.Movimientos.Count);

            var credito = cuenta.Movimientos.Last();

            Assert.Equal(TipoMovimientoCuenta.Credito, credito.Tipo);
            Assert.Equal(50000m, credito.Importe);
        }

        [Fact]
        public void RegistrarCredito_SiSuperaDeuda_DebeFallar()
        {
            // Arrange
            var cuenta = new CuentaCorriente(
                clienteId: 1,
                limiteDeuda: 500000m);

            cuenta.RegistrarDebito(
                100000m,
                "Venta");

            // Act
            Assert.Throws<InvalidOperationException>(() =>
                cuenta.RegistrarCredito(
                    150000m,
                    "Pago excesivo"));

            // Assert
            Assert.Equal(100000m, cuenta.SaldoActual);
            Assert.Single(cuenta.Movimientos);
        }

        [Fact]
        public void CuentaSuspendida_NoDebePermitirNuevosDebitos()
        {
            // Arrange
            var cuenta = new CuentaCorriente(
                clienteId: 1,
                limiteDeuda: 500000m);

            cuenta.Suspender();

            // Act
            Assert.Throws<InvalidOperationException>(() =>
                cuenta.RegistrarDebito(
                    100000m,
                    "Nueva venta"));

            // Assert
            Assert.Equal(
                EstadoCuentaCorriente.Suspendida,
                cuenta.Estado);

            Assert.Empty(cuenta.Movimientos);
        }

        [Fact]
        public void CuentaSuspendida_DebePermitirRegistrarCreditos()
        {
            // Arrange
            var cuenta = new CuentaCorriente(
                clienteId: 1,
                limiteDeuda: 500000m);

            cuenta.RegistrarDebito(
                100000m,
                "Venta");

            cuenta.Suspender();

            // Act
            cuenta.RegistrarCredito(
                40000m,
                "Pago parcial");

            // Assert
            Assert.Equal(60000m, cuenta.SaldoActual);
            Assert.Equal(2, cuenta.Movimientos.Count);
        }

        [Fact]
        public void CerrarCuenta_ConDeudaPendiente_DebeFallar()
        {
            // Arrange
            var cuenta = new CuentaCorriente(
                clienteId: 1,
                limiteDeuda: 500000m);

            cuenta.RegistrarDebito(
                100000m,
                "Venta");

            // Act
            Assert.Throws<InvalidOperationException>(() =>
                cuenta.Cerrar());

            // Assert
            Assert.Equal(
                EstadoCuentaCorriente.Activa,
                cuenta.Estado);
        }

        [Fact]
        public void CerrarCuenta_SinDeuda_DebeCambiarEstado()
        {
            // Arrange
            var cuenta = new CuentaCorriente(
                clienteId: 1,
                limiteDeuda: 500000m);

            cuenta.RegistrarDebito(
                100000m,
                "Venta");

            cuenta.RegistrarCredito(
                100000m,
                "Pago total");

            // Act
            cuenta.Cerrar();

            // Assert
            Assert.Equal(0m, cuenta.SaldoActual);

            Assert.Equal(
                EstadoCuentaCorriente.Cerrada,
                cuenta.Estado);
        }

        [Fact]
        public void CrearCuenta_ConLimiteNegativo_DebeFallar()
        {
            Assert.Throws<ArgumentException>(() =>
                new CuentaCorriente(
                    clienteId: 1,
                    limiteDeuda: -1000m));
        }

        [Fact]
        public void ReducirLimite_NoDebeModificarDeudaExistente()
        {
            // Arrange
            var cuenta = new CuentaCorriente(
                clienteId: 1,
                limiteDeuda: 500000m);

            cuenta.RegistrarDebito(
                300000m,
                "Venta");

            // Act
            cuenta.ModificarLimiteDeuda(200000m);

            // Assert
            Assert.Equal(200000m, cuenta.LimiteDeuda);
            Assert.Equal(300000m, cuenta.SaldoActual);

            Assert.False(cuenta.PuedeRegistrarDeuda(10000m));
        }



        [Fact]
        public void RegistrarPagoDeVenta_SiVentaTieneDebitoRevertido_DebeFallar()
        {
            // Arrange
            var cuenta = new CuentaCorriente(
                clienteId: 1,
                limiteDeuda: 500000m);

            var categoria = new Categoria(
                "Periféricos",
                "Productos periféricos");

            var producto = new Producto(
                codigoInterno: "TEC-CC-001",
                nombre: "Teclado",
                precioVenta: 100000m,
                costo: 50000m,
                stockMinimo: 2m,
                categoria: categoria);

            var venta = new Venta(1);

            venta.AgregarDetalle(
                producto,
                cantidad: 1m,
                precioUnitario: 100000m);

            venta.Confirmar();

            typeof(Venta)
                .GetProperty(nameof(Venta.Id))!
                .SetValue(venta, 1);

            cuenta.RegistrarDebito(
                100000m,
                "Venta a cuenta corriente",
                venta);

            cuenta.RevertirDebitoDeVenta(venta);

            var pago = new Pago(
                importe: 10000m,
                formaPago: FormaPago.Efectivo,
                usuarioId: 1);

            typeof(Pago)
                .GetProperty(nameof(Pago.Id))!
                .SetValue(pago, 1);

            // Act
            var excepcion = Assert.Throws<InvalidOperationException>(() =>
                cuenta.RegistrarPagoDeVenta(
                    venta,
                    pago,
                    10000m));

            // Assert
            Assert.Equal(
                "No se pueden aplicar pagos a una venta cuyo débito fue revertido.",
                excepcion.Message);

            Assert.Equal(0m, cuenta.SaldoActual);
            Assert.Equal(2, cuenta.Movimientos.Count);
        }


        [Fact]
        public void TieneAnulacionDeVenta_DebeDistinguirCreditoDeAnulacion()
        {
            // Arrange
            var cuenta = new CuentaCorriente(
                clienteId: 1,
                limiteDeuda: 500000m);

            var categoria = new Categoria(
                "Periféricos",
                "Productos periféricos");

            var producto = new Producto(
                codigoInterno: "TEC-CC-002",
                nombre: "Teclado",
                precioVenta: 100000m,
                costo: 50000m,
                stockMinimo: 2m,
                categoria: categoria);

            var venta = new Venta(1);

            venta.AgregarDetalle(
                producto,
                cantidad: 1m,
                precioUnitario: 100000m);

            venta.Confirmar();

            typeof(Venta)
                .GetProperty(nameof(Venta.Id))!
                .SetValue(venta, 1);

            cuenta.RegistrarDebito(
                100000m,
                "Venta a cuenta corriente",
                venta);

            // Act y Assert: todavía no hay anulación.
            Assert.False(cuenta.TieneAnulacionDeVenta(venta.Id));

            // Un crédito común no debe interpretarse como anulación.
            cuenta.RegistrarCredito(
                20000m,
                "Crédito manual",
                venta);

            Assert.True(cuenta.TieneCreditoDeVenta(venta.Id));
            Assert.False(cuenta.TieneAnulacionDeVenta(venta.Id));

            // Comprobamos que el saldo no cambió inesperadamente.
            Assert.Equal(80000m, cuenta.SaldoActual);
        }


        [Fact]
        public void RegistrarNotaCredito_DebeReducirSaldoYVincularDocumento()
        {
            // Arrange
            var cuenta = new CuentaCorriente(
                clienteId: 1,
                limiteDeuda: 500000m);

            var categoria = new Categoria(
                "Periféricos",
                "Productos periféricos");

            var producto = new Producto(
                codigoInterno: "TEC-NC-001",
                nombre: "Teclado",
                precioVenta: 100000m,
                costo: 50000m,
                stockMinimo: 2m,
                categoria: categoria);

            var venta = new Venta(1);

            venta.AgregarDetalle(
                producto,
                cantidad: 1m,
                precioUnitario: 100000m);

            venta.Confirmar();

            typeof(Venta)
                .GetProperty(nameof(Venta.Id))!
                .SetValue(venta, 1);

            cuenta.RegistrarDebito(
                100000m,
                "Venta a cuenta corriente",
                venta);

            var notaCredito = new NotaCredito(
                facturaId: 1,
                numero: "INT-NC-PRUEBA-001",
                motivo: "Descuento comercial",
                importeTotal: 30000m);

            // Act
            cuenta.RegistrarNotaCredito(notaCredito, venta);

            // Assert
            Assert.Equal(70000m, cuenta.SaldoActual);

            Assert.Equal(
                70000m,
                cuenta.ObtenerSaldoPendienteDeVenta(venta.Id));

            Assert.Equal(2, cuenta.Movimientos.Count);

            var movimiento = cuenta.Movimientos.Last();

            Assert.Equal(
                TipoMovimientoCuenta.Credito,
                movimiento.Tipo);

            Assert.Equal(
                OrigenMovimientoCuenta.NotaCredito,
                movimiento.Origen);

            Assert.Equal(30000m, movimiento.Importe);

            Assert.Equal(venta.Id, movimiento.VentaId);

            Assert.Same(
                notaCredito,
                movimiento.DocumentoComercial);
        }


        [Fact]
        public void RegistrarNotaCredito_SiYaFueAplicada_DebeFallar()
        {
            // Arrange
            var cuenta = new CuentaCorriente(1, 500000m);

            var categoria = new Categoria(
                "Periféricos",
                "Productos periféricos");

            var producto = new Producto(
                "TEC-NC-002",
                "Teclado",
                100000m,
                50000m,
                2m,
                categoria);

            var venta = new Venta(1);

            venta.AgregarDetalle(producto, 1m, 100000m);
            venta.Confirmar();

            typeof(Venta)
                .GetProperty(nameof(Venta.Id))!
                .SetValue(venta, 1);

            cuenta.RegistrarDebito(
                100000m,
                "Venta a cuenta corriente",
                venta);

            var notaCredito = new NotaCredito(
                facturaId: 1,
                numero: "INT-NC-PRUEBA-002",
                motivo: "Descuento",
                importeTotal: 30000m);

            // Act
            cuenta.RegistrarNotaCredito(notaCredito, venta);

            var excepcion = Assert.Throws<InvalidOperationException>(() =>
                cuenta.RegistrarNotaCredito(notaCredito, venta));

            // Assert
            Assert.Equal(
                "La Nota de Crédito ya fue aplicada a esta cuenta.",
                excepcion.Message);

            Assert.Equal(70000m, cuenta.SaldoActual);
            Assert.Equal(2, cuenta.Movimientos.Count);
        }


    }
}
