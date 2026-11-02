
using SistemaVenta.Application.Tests.Fakes;
using SistemaVenta.Application.UseCases.DocumentosComerciales;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Application.Tests.DocumentosComerciales
{
    public class EmitirNotaCreditoTests
    {
        [Fact]
        public async Task EmitirNotaCredito_FacturaInexistente_LanzaExcepcion()
        {
            // Arrange
            var ventas = new FakeVentaRepository();
            var documentos = new FakeDocumentoComercialRepository();
            var unitOfWork = new FakeUnitOfWork();
            var transaccion = new FakeTransaccionVenta();
            var cuentas = new FakeCuentaCorrienteRepository();

            var casoDeUso = new EmitirNotaCredito(
                ventas,
                documentos,
                unitOfWork,
                transaccion,
                cuentas);

            // Act
            var excepcion = await Assert.ThrowsAsync<InvalidOperationException>(
                () => casoDeUso.EjecutarAsync(
                    facturaId: 999,
                    motivo: "Corrección de importe",
                    importeTotal: 1000m));

            // Assert
            Assert.Equal(
                "La factura no existe.",
                excepcion.Message);

            Assert.Null(documentos.DocumentoAgregado);
            Assert.False(unitOfWork.GuardarCambiosFueLlamado);
        }

        [Fact]
        public async Task EmitirNotaCredito_FacturaAnulada_LanzaExcepcion()
        {
            // Arrange
            var ventas = new FakeVentaRepository();
            var documentos = new FakeDocumentoComercialRepository();
            var unitOfWork = new FakeUnitOfWork();
            var transaccion = new FakeTransaccionVenta();
            var cuentas = new FakeCuentaCorrienteRepository();

            var factura = new Factura(
                ventaId: 1,
                tipo: TipoFactura.B,
                importeTotal: 10000m,
                numero: "FAC-001");

            AsignarId(factura, 1);

            factura.Anular();

            documentos.Documento = factura;

            var casoDeUso = new EmitirNotaCredito(
                ventas,
                documentos,
                unitOfWork,
                transaccion,
                cuentas);

            // Act
            var excepcion = await Assert.ThrowsAsync<InvalidOperationException>(
                () => casoDeUso.EjecutarAsync(
                    facturaId: 1,
                    motivo: "Corrección de importe",
                    importeTotal: 2000m));

            // Assert
            Assert.Equal(
                "La factura no está emitida.",
                excepcion.Message);

            Assert.Null(documentos.DocumentoAgregado);
            Assert.False(unitOfWork.GuardarCambiosFueLlamado);
        }

        [Fact]
        public async Task EmitirNotaCredito_VentaAnulada_LanzaExcepcion()
        {
            // Arrange
            var ventas = new FakeVentaRepository();
            var documentos = new FakeDocumentoComercialRepository();
            var unitOfWork = new FakeUnitOfWork();
            var transaccion = new FakeTransaccionVenta();
            var cuentas = new FakeCuentaCorrienteRepository();

            var factura = new Factura(
                ventaId: 1,
                tipo: TipoFactura.B,
                importeTotal: 10000m,
                numero: "FAC-002");

            AsignarId(factura, 1);

            documentos.Documento = factura;

            var categoria = new Categoria(
                "Periféricos",
                "Productos periféricos");

            var producto = new Producto(
                codigoInterno: "TEC-NC-001",
                nombre: "Teclado",
                precioVenta: 10000m,
                costo: 5000m,
                stockMinimo: 2m,
                categoria: categoria);

            var venta = new Venta(1);

            // Agregamos un producto para que la venta sea válida.
            venta.AgregarDetalle(
                producto,
                cantidad: 1m,
                precioUnitario: 10000m);

            // Ahora podemos confirmarla y anularla.
            venta.Confirmar();
            venta.Anular();

            // El fake le asignará automáticamente el ID 1.
            await ventas.AgregarAsync(venta);

            var casoDeUso = new EmitirNotaCredito(
                ventas,
                documentos,
                unitOfWork,
                transaccion,
                cuentas);

            // Act
            var excepcion = await Assert.ThrowsAsync<InvalidOperationException>(
                () => casoDeUso.EjecutarAsync(
                    facturaId: 1,
                    motivo: "Corrección de importe",
                    importeTotal: 2000m));

            // Assert
            Assert.Equal(
                "La venta asociada no está confirmada.",
                excepcion.Message);

            Assert.Null(documentos.DocumentoAgregado);
            Assert.False(unitOfWork.GuardarCambiosFueLlamado);
        }


        [Fact]
        public async Task EmitirNotaCredito_ImporteSuperiorAlDisponible_LanzaExcepcion()
        {
            // Arrange
            var ventas = new FakeVentaRepository();
            var documentos = new FakeDocumentoComercialRepository();
            var unitOfWork = new FakeUnitOfWork();
            var transaccion = new FakeTransaccionVenta();
            var cuentas = new FakeCuentaCorrienteRepository();

            var factura = new Factura(
                ventaId: 1,
                tipo: TipoFactura.B,
                importeTotal: 100000m,
                numero: "FAC-003");

            AsignarId(factura, 1);
            documentos.Documento = factura;

            // Simulamos notas anteriores.
            documentos.TotalNotasCredito = 30000m;
            documentos.TotalNotasDebito = 0m;

            // Creamos una venta confirmada.
            var categoria = new Categoria(
                "Periféricos",
                "Productos periféricos");

            var producto = new Producto(
                codigoInterno: "TEC-NC-002",
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

            await ventas.AgregarAsync(venta);

            var casoDeUso = new EmitirNotaCredito(
                ventas,
                documentos,
                unitOfWork,
                transaccion,
                cuentas);

            // Act
            var excepcion =
                await Assert.ThrowsAsync<InvalidOperationException>(
                    () => casoDeUso.EjecutarAsync(
                        facturaId: 1,
                        motivo: "Corrección de importe",
                        importeTotal: 80000m));

            // Assert
            Assert.Equal(
                "El importe de la Nota de Crédito supera " +
                "el importe disponible de la factura.",
                excepcion.Message);

            Assert.Null(documentos.DocumentoAgregado);
            Assert.False(unitOfWork.GuardarCambiosFueLlamado);
        }


        [Fact]
        public async Task EmitirNotaCredito_DatosValidos_GuardaNotaCredito()
        {
            // Arrange
            var ventas = new FakeVentaRepository();
            var documentos = new FakeDocumentoComercialRepository();
            var unitOfWork = new FakeUnitOfWork();
            var transaccion = new FakeTransaccionVenta();
            var cuentas = new FakeCuentaCorrienteRepository();

            var factura = new Factura(
                ventaId: 1,
                tipo: TipoFactura.B,
                importeTotal: 100000m,
                numero: "FAC-004");

            AsignarId(factura, 1);
            documentos.Documento = factura;

            documentos.TotalNotasCredito = 20000m;
            documentos.TotalNotasDebito = 5000m;

            var categoria = new Categoria(
                "Periféricos",
                "Productos periféricos");

            var producto = new Producto(
                codigoInterno: "TEC-NC-003",
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

            await ventas.AgregarAsync(venta);

            var casoDeUso = new EmitirNotaCredito(
                ventas,
                documentos,
                unitOfWork,
                transaccion,
                cuentas);

            // Act
            await casoDeUso.EjecutarAsync(
                facturaId: 1,
                motivo: "Descuento posterior a la facturación",
                importeTotal: 30000m);

            // Assert
            var nota = Assert.IsType<NotaCredito>(
                documentos.DocumentoAgregado);

            Assert.Equal(1, nota.FacturaId);
            Assert.Equal(30000m, nota.ImporteTotal);

            Assert.Equal(
                "Descuento posterior a la facturación",
                nota.Motivo);

            Assert.Equal(
                EstadoDocumento.Emitido,
                nota.Estado);

            Assert.StartsWith("INT-NC-", nota.Numero);

            Assert.True(
                unitOfWork.GuardarCambiosFueLlamado);

            // La factura original no debe modificarse.
            Assert.Equal(100000m, factura.ImporteTotal);
        }





        private static void AsignarId(DocumentoComercial documento, int id)
        {
            typeof(DocumentoComercial)
                .GetProperty(nameof(DocumentoComercial.Id))!
                .SetValue(documento, id);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1000)]
        public async Task EmitirNotaCredito_ImporteInvalido_LanzaExcepcion(
            decimal importe)
        {
            // Arrange
            var ventas = new FakeVentaRepository();
            var documentos = new FakeDocumentoComercialRepository();
            var unitOfWork = new FakeUnitOfWork();
            var transaccion = new FakeTransaccionVenta();
            var cuentas = new FakeCuentaCorrienteRepository();

            var casoDeUso = new EmitirNotaCredito(
                 ventas,
                 documentos,
                 unitOfWork,
                 transaccion,
                 cuentas);

            // Act
            var excepcion =
                await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                    () => casoDeUso.EjecutarAsync(
                        facturaId: 1,
                        motivo: "Corrección de importe",
                        importeTotal: importe));

            // Assert
            Assert.Equal("importeTotal", excepcion.ParamName);

            Assert.Null(documentos.DocumentoAgregado);
            Assert.False(unitOfWork.GuardarCambiosFueLlamado);
        }


        [Fact]
        public async Task EmitirNotaCredito_DebeReducirSaldoCuentaCorriente()
        {
            // Arrange
            var ventas = new FakeVentaRepository();
            var documentos = new FakeDocumentoComercialRepository();
            var unitOfWork = new FakeUnitOfWork();
            var transaccion = new FakeTransaccionVenta();
            var cuentas = new FakeCuentaCorrienteRepository();

            var categoria = new Categoria(
                "Periféricos", "Productos periféricos");

            var producto = new Producto(
                "TEC-NC-004", "Teclado",
                100000m, 50000m, 2m, categoria);

            var venta = new Venta(1);
            venta.AgregarDetalle(producto, 1m, 100000m);
            venta.Confirmar();

            await ventas.AgregarAsync(venta);

            var cuenta = new CuentaCorriente(1, 500000m);
            cuenta.RegistrarDebito(
                100000m, "Venta a cuenta corriente", venta);

            cuentas.ConfigurarCuenta(1, cuenta);

            var factura = new Factura(
                venta.Id, TipoFactura.B, 100000m, "FAC-005");

            AsignarId(factura, 1);
            documentos.Documento = factura;

            var casoDeUso = new EmitirNotaCredito(
                ventas, documentos, unitOfWork,
                transaccion, cuentas);

            // Act
            await casoDeUso.EjecutarAsync(
                facturaId: 1,
                motivo: "Descuento comercial",
                importeTotal: 30000m);

            // Assert
            Assert.Equal(70000m, cuenta.SaldoActual);
            Assert.Equal(
                70000m,
                cuenta.ObtenerSaldoPendienteDeVenta(venta.Id));

            Assert.Contains(cuenta.Movimientos, m =>
                m.Origen == OrigenMovimientoCuenta.NotaCredito &&
                m.Importe == 30000m);
        }



    }
}
