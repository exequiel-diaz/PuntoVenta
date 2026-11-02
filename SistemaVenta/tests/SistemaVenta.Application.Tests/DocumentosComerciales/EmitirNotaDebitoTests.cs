
using SistemaVenta.Application.Tests.Fakes;
using SistemaVenta.Application.UseCases.DocumentosComerciales;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Application.Tests.DocumentosComerciales;

public class EmitirNotaDebitoTests
{
    [Fact]
    public async Task EmitirNotaDebito_DebeAumentarSaldoCuentaCorriente()
    {
        // Arrange
        var ventas = new FakeVentaRepository();
        var documentos = new FakeDocumentoComercialRepository();
        var cuentas = new FakeCuentaCorrienteRepository();
        var unitOfWork = new FakeUnitOfWork();
        var transaccion = new FakeTransaccionVenta();

        var categoria = new Categoria(
            "Periféricos", "Productos periféricos");

        var producto = new Producto(
            "TEC-ND-001", "Teclado",
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
            venta.Id, TipoFactura.B, 100000m, "FAC-ND-001");

        typeof(DocumentoComercial)
            .GetProperty(nameof(DocumentoComercial.Id))!
            .SetValue(factura, 1);

        documentos.Documento = factura;

        var casoDeUso = new EmitirNotaDebito(
            ventas, documentos, cuentas,
            unitOfWork, transaccion);

        // Act
        await casoDeUso.EjecutarAsync(
            facturaId: 1,
            motivo: "Cargo adicional",
            importeTotal: 20000m);

        // Assert
        Assert.Equal(120000m, cuenta.SaldoActual);

        Assert.Contains(cuenta.Movimientos, m =>
            m.Origen == OrigenMovimientoCuenta.NotaDebito &&
            m.Importe == 20000m);

        Assert.IsType<NotaDebito>(
            documentos.DocumentoAgregado);
    }
}
