
using SistemaVenta.Application.Tests.Fakes;
using SistemaVenta.Application.UseCases.Pagos;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;
using Xunit;

namespace SistemaVenta.Application.Tests.Pagos
{
    public class AplicarPagoTests
    {
        [Fact]
        public async Task AplicarPago_ImporteCero_DebeLanzarExcepcion()
        {
            // Arrange
            var pagoRepository = new FakePagoRepository();
            var documentoRepository =
                new FakeDocumentoComercialRepository();

            var unitOfWork = new FakeUnitOfWork();

            var transaccion =
                new FakeTransaccionAplicacionPago();

            var casoDeUso = new AplicarPago(
                pagoRepository,
                documentoRepository,
                unitOfWork,
                transaccion);

            // Act
            await Assert.ThrowsAsync<ArgumentException>(
                () => casoDeUso.EjecutarAsync(
                    pagoId: 1,
                    documentoComercialId: 1,
                    importeAplicado: 0m));

            // Assert
            Assert.False(unitOfWork.GuardarCambiosFueLlamado);
        }


        [Fact]
        public async Task AplicarPago_ImporteNegativo_DebeLanzarExcepcion()
        {
            // Arrange
            var pagoRepository = new FakePagoRepository();
            var documentoRepository =
                new FakeDocumentoComercialRepository();

            var unitOfWork = new FakeUnitOfWork();

            var transaccion =
                new FakeTransaccionAplicacionPago();

            var casoDeUso = new AplicarPago(
                pagoRepository,
                documentoRepository,
                unitOfWork,
                transaccion);

            // Act
            await Assert.ThrowsAsync<ArgumentException>(
                () => casoDeUso.EjecutarAsync(
                    pagoId: 1,
                    documentoComercialId: 1,
                    importeAplicado: -100m));

            // Assert
            Assert.False(unitOfWork.GuardarCambiosFueLlamado);
        }


        [Fact]
        public async Task AplicarPago_PagoInexistente_DebeLanzarExcepcion()
        {
            // Arrange
            var pagoRepository = new FakePagoRepository();

            var documentoRepository =
                new FakeDocumentoComercialRepository();

            var unitOfWork = new FakeUnitOfWork();

            var transaccion =
                new FakeTransaccionAplicacionPago();

            var casoDeUso = new AplicarPago(
                pagoRepository,
                documentoRepository,
                unitOfWork,
                transaccion);

            // Act
            var excepcion =
                await Assert.ThrowsAsync<InvalidOperationException>(
                    () => casoDeUso.EjecutarAsync(
                        pagoId: 999,
                        documentoComercialId: 1,
                        importeAplicado: 1000m));

            // Assert
            Assert.Equal(
                "El pago no existe.",
                excepcion.Message);

            Assert.False(unitOfWork.GuardarCambiosFueLlamado);
        }


        [Fact]
        public async Task AplicarPago_PagoAnulado_DebeLanzarExcepcion()
        {
            // Arrange
            var pagoRepository = new FakePagoRepository();

            var documentoRepository =
                new FakeDocumentoComercialRepository();

            var unitOfWork = new FakeUnitOfWork();

            var pago = new Pago(
                10000m,
                FormaPago.Efectivo,
                1);

            pago.Anular();

            await pagoRepository.AgregarAsync(pago);

            var transaccion =
                new FakeTransaccionAplicacionPago();

            var casoDeUso = new AplicarPago(
                pagoRepository,
                documentoRepository,
                unitOfWork,
                transaccion);

            // Act
            var excepcion =
                await Assert.ThrowsAsync<InvalidOperationException>(
                    () => casoDeUso.EjecutarAsync(
                        pagoId: pago.Id,
                        documentoComercialId: 1,
                        importeAplicado: 5000m));

            // Assert
            Assert.Equal(
                "No se puede aplicar un pago anulado.",
                excepcion.Message);

            Assert.Empty(pago.Aplicaciones);

            Assert.False(unitOfWork.GuardarCambiosFueLlamado);
        }


        [Fact]
        public async Task AplicarPago_FacturaInexistente_DebeLanzarExcepcion()
        {
            // Arrange
            var pagoRepository = new FakePagoRepository();

            var documentoRepository =
                new FakeDocumentoComercialRepository();

            var unitOfWork = new FakeUnitOfWork();

            var pago = new Pago(
                10000m,
                FormaPago.Efectivo,
                1);

            await pagoRepository.AgregarAsync(pago);

            var transaccion =
                new FakeTransaccionAplicacionPago();

            var casoDeUso = new AplicarPago(
                pagoRepository,
                documentoRepository,
                unitOfWork,
                transaccion);

            // Act
            var excepcion =
                await Assert.ThrowsAsync<InvalidOperationException>(
                    () => casoDeUso.EjecutarAsync(
                        pagoId: pago.Id,
                        documentoComercialId: 999,
                        importeAplicado: 5000m));

            // Assert
            Assert.Equal(
                "El documento no existe o no es una factura.",
                excepcion.Message);

            Assert.Empty(pago.Aplicaciones);

            Assert.Equal(10000m, pago.ImporteDisponible);

            Assert.False(unitOfWork.GuardarCambiosFueLlamado);
        }


        [Fact]
        public async Task AplicarPago_FacturaAnulada_DebeLanzarExcepcion()
        {
            // Arrange
            var pagoRepository = new FakePagoRepository();

            var documentoRepository =
                new FakeDocumentoComercialRepository();

            var unitOfWork = new FakeUnitOfWork();

            var pago = new Pago(
                10000m,
                FormaPago.Efectivo,
                1);

            await pagoRepository.AgregarAsync(pago);

            var factura = new Factura(
                ventaId: 1,
                tipo: TipoFactura.B,
                importeTotal: 20000m,
                numero: "0001-00000001");

            factura.Anular();

            documentoRepository.Documento = factura;

            var transaccion =
                new FakeTransaccionAplicacionPago();

            var casoDeUso = new AplicarPago(
                pagoRepository,
                documentoRepository,
                unitOfWork,
                transaccion);

            // Act
            var excepcion =
                await Assert.ThrowsAsync<InvalidOperationException>(
                    () => casoDeUso.EjecutarAsync(
                        pagoId: pago.Id,
                        documentoComercialId: factura.Id,
                        importeAplicado: 5000m));

            // Assert
            Assert.Equal(
                "La factura debe estar emitida.",
                excepcion.Message);

            Assert.Empty(pago.Aplicaciones);

            Assert.Equal(10000m, pago.ImporteDisponible);

            Assert.False(unitOfWork.GuardarCambiosFueLlamado);
        }


        [Fact]
        public async Task AplicarPago_SuperaSaldoPendiente_DebeLanzarExcepcion()
        {
            // Arrange
            var pagoRepository = new FakePagoRepository();

            var documentoRepository =
                new FakeDocumentoComercialRepository();

            var unitOfWork = new FakeUnitOfWork();

            var pago = new Pago(
                40000m,
                FormaPago.Efectivo,
                1);

            await pagoRepository.AgregarAsync(pago);

            var factura = new Factura(
                ventaId: 1,
                tipo: TipoFactura.B,
                importeTotal: 50000m,
                numero: "0001-00000002");

            documentoRepository.Documento = factura;

            documentoRepository.TotalAplicado = 30000m;

            var transaccion =
                new FakeTransaccionAplicacionPago();

            var casoDeUso = new AplicarPago(
                pagoRepository,
                documentoRepository,
                unitOfWork,
                transaccion);

            // Act
            var excepcion =
                await Assert.ThrowsAsync<InvalidOperationException>(
                    () => casoDeUso.EjecutarAsync(
                        pagoId: pago.Id,
                        documentoComercialId: factura.Id,
                        importeAplicado: 25000m));

            // Assert
            Assert.Equal(
                "El importe supera el saldo pendiente de la factura.",
                excepcion.Message);

            Assert.Empty(pago.Aplicaciones);

            Assert.Equal(40000m, pago.ImporteDisponible);

            Assert.False(unitOfWork.GuardarCambiosFueLlamado);
        }


        [Fact]
        public async Task AplicarPago_SuperaImporteDisponible_DebeLanzarExcepcion()
        {
            // Arrange
            var pagoRepository = new FakePagoRepository();

            var documentoRepository =
                new FakeDocumentoComercialRepository();

            var unitOfWork = new FakeUnitOfWork();

            var pago = new Pago(
                30000m,
                FormaPago.Efectivo,
                1);

            await pagoRepository.AgregarAsync(pago);

            var factura = new Factura(
                ventaId: 1,
                tipo: TipoFactura.B,
                importeTotal: 50000m,
                numero: "0001-00000003");

            documentoRepository.Documento = factura;
            documentoRepository.TotalAplicado = 0m;

            var transaccion =
                new FakeTransaccionAplicacionPago();

            var casoDeUso = new AplicarPago(
                pagoRepository,
                documentoRepository,
                unitOfWork,
                transaccion);

            // Act
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => casoDeUso.EjecutarAsync(
                    pagoId: pago.Id,
                    documentoComercialId: factura.Id,
                    importeAplicado: 40000m));

            // Assert
            Assert.Empty(pago.Aplicaciones);

            Assert.Equal(30000m, pago.ImporteDisponible);

            Assert.False(unitOfWork.GuardarCambiosFueLlamado);
        }


        [Fact]
        public async Task AplicarPago_Valido_DebeRegistrarAplicacionYGuardarCambios()
        {
            // Arrange
            var pagoRepository = new FakePagoRepository();

            var documentoRepository =
                new FakeDocumentoComercialRepository();

            var unitOfWork = new FakeUnitOfWork();

            var pago = new Pago(
                30000m,
                FormaPago.Transferencia,
                1);

            await pagoRepository.AgregarAsync(pago);

            var factura = new Factura(
                ventaId: 1,
                tipo: TipoFactura.B,
                importeTotal: 50000m,
                numero: "0001-00000004");

            typeof(DocumentoComercial)
                .GetProperty(nameof(DocumentoComercial.Id))!
                .SetValue(factura, 10);

            documentoRepository.Documento = factura;
            documentoRepository.TotalAplicado = 10000m;

            var transaccion =
                new FakeTransaccionAplicacionPago();

            var casoDeUso = new AplicarPago(
                pagoRepository,
                documentoRepository,
                unitOfWork,
                transaccion);

            // Act
            await casoDeUso.EjecutarAsync(
                pagoId: pago.Id,
                documentoComercialId: factura.Id,
                importeAplicado: 20000m);

            // Assert
            var aplicacion = Assert.Single(pago.Aplicaciones);

            Assert.Equal(
                factura.Id,
                aplicacion.DocumentoComercialId);

            Assert.Equal(
                20000m,
                aplicacion.ImporteAplicado);

            Assert.Same(pago, aplicacion.Pago);

            Assert.Equal(
                10000m,
                pago.ImporteDisponible);

            Assert.True(
                unitOfWork.GuardarCambiosFueLlamado);
        }

    }
}
