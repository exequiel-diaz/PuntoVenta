
using SistemaVenta.Application.Tests.Fakes;
using SistemaVenta.Application.UseCases.Facturas;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;
using Xunit;

namespace SistemaVenta.Application.Tests.UseCases.Facturas
{
    public class EmitirFacturaTests
    {
        [Fact]
        public async Task EjecutarAsync_VentaInexistente_LanzaExcepcion()
        {
            // Arrange
            var ventas = new FakeVentaRepository();
            var documentos = new FakeDocumentoComercialRepository();
            var unitOfWork = new FakeUnitOfWork();

            var emitirFactura = new EmitirFactura(
                ventas,
                documentos,
                unitOfWork,
                new FakeTransaccionVenta());

            // Act
            var excepcion = await Assert.ThrowsAsync<InvalidOperationException>(
                () => emitirFactura.EjecutarAsync(
                    999,
                    SistemaVenta.Domain.Enums.TipoFactura.B));

            // Assert
            Assert.Equal(
                "La venta no existe.",
                excepcion.Message);
        }

        [Fact]
        public async Task EjecutarAsync_VentaPendiente_LanzaExcepcion()
        {
            // Arrange
            var ventas = new FakeVentaRepository();
            var documentos = new FakeDocumentoComercialRepository();
            var unitOfWork = new FakeUnitOfWork();

            // Creamos una venta nueva.
            var venta = new Venta(1);

            // El estado inicial de una venta es Pendiente.
            await ventas.AgregarAsync(venta);

            var emitirFactura = new EmitirFactura(
                ventas,
                documentos,
                unitOfWork,
                new FakeTransaccionVenta());

            // Act
            var excepcion = await Assert.ThrowsAsync<InvalidOperationException>(
                () => emitirFactura.EjecutarAsync(
                    venta.Id,
                    TipoFactura.B));

            // Assert
            Assert.Equal(
                "Solo se pueden facturar ventas confirmadas.",
                excepcion.Message);
        }

        [Fact]
        public async Task EjecutarAsync_VentaYaFacturada_LanzaExcepcion()
        {
            // Arrange
            var ventas = new FakeVentaRepository();
            var documentos = new FakeDocumentoComercialRepository();
            var unitOfWork = new FakeUnitOfWork();

            // 1. Crear una categoría y un producto.
            var categoria = new Categoria("Componentes");

            var producto = new Producto(
                "PROD-001",
                "Memoria RAM 8 GB",
                1000m,
                600m,
                2m,
                categoria);

            // 2. Crear una venta con un producto.
            var venta = new Venta(1);

            venta.AgregarDetalle(
                producto,
                1m,
                1000m);

            // 3. Confirmar la venta.
            venta.Confirmar();

            // 4. Guardarla en el repositorio falso.
            await ventas.AgregarAsync(venta);

            // 5. Simular que ya existe una factura.
            documentos.Factura = new Factura(
                venta.Id,
                TipoFactura.B,
                venta.Total,
                "INT-FACTURA-EXISTENTE");

            var emitirFactura = new EmitirFactura(
                ventas,
                documentos,
                unitOfWork,
                new FakeTransaccionVenta());

            // Act
            var excepcion =
                await Assert.ThrowsAsync<InvalidOperationException>(
                    () => emitirFactura.EjecutarAsync(
                        venta.Id,
                        TipoFactura.B));

            // Assert
            Assert.Equal(
                "La venta ya tiene una factura registrada.",
                excepcion.Message);

            Assert.Null(documentos.DocumentoAgregado);
        }

        [Fact]
        public async Task EjecutarAsync_VentaConfirmadaSinFactura_CreaFacturaCorrectamente()
        {
            // Arrange
            var ventas = new FakeVentaRepository();
            var documentos = new FakeDocumentoComercialRepository();
            var unitOfWork = new FakeUnitOfWork();

            var categoria = new Categoria("Componentes");

            var producto = new Producto(
                "PROD-002",
                "Disco SSD 480 GB",
                25000m,
                18000m,
                2m,
                categoria);

            var venta = new Venta(1);

            venta.AgregarDetalle(
                producto,
                2m,
                25000m);

            venta.Confirmar();

            await ventas.AgregarAsync(venta);

            var emitirFactura = new EmitirFactura(
                ventas,
                documentos,
                unitOfWork,
                new FakeTransaccionVenta());

            // Act
            await emitirFactura.EjecutarAsync(
                venta.Id,
                TipoFactura.B);

            // Assert
            var factura = Assert.IsType<Factura>(
                documentos.DocumentoAgregado);

            Assert.Equal(venta.Id, factura.VentaId);
            Assert.Equal(TipoFactura.B, factura.Tipo);
            Assert.Equal(50000m, factura.ImporteTotal);

            Assert.StartsWith("INT-", factura.Numero);
            Assert.Equal(EstadoDocumento.Emitido, factura.Estado);

            //verifico que se solicitaron los cambios.
            Assert.True(unitOfWork.GuardarCambiosFueLlamado);
        }


    }
}
