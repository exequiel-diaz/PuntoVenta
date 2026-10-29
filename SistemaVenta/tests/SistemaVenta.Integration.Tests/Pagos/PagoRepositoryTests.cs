
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;
using SistemaVenta.Infrastructure.Persistence;
using SistemaVenta.Infrastructure.Repositories;
using SistemaVenta.Integration.Tests.Infrastructure;
using Xunit;

namespace SistemaVenta.Integration.Tests.Pagos;

public class PagoRepositoryTests
{
    [Fact]
    public async Task AgregarPago_DebePersistirYRecuperarPago()
    {
        // Arrange
        await using var aplicacion =
            new SistemaVentaWebApplicationFactory();

        using var scope =
            aplicacion.Services.CreateScope();

        var contexto = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        await using var transaccion =
            await contexto.Database.BeginTransactionAsync();

        var usuario = new Usuario(
            $"test_pago_{Guid.NewGuid():N}",
            "Usuario de prueba",
            "hash-de-prueba-no-utilizado-para-login",
            RolUsuario.Vendedor);

        contexto.Set<Usuario>().Add(usuario);
        await contexto.SaveChangesAsync();

        var pago = new Pago(
            15000m,
            FormaPago.Efectivo,
            usuario.Id,
            "Pago de prueba de integración");

        var repositorio = new PagoRepository(contexto);

        // Act
        await repositorio.AgregarAsync(pago);
        await contexto.SaveChangesAsync();

        var pagoId = pago.Id;

        // Evitamos recuperar la entidad desde el
        // seguimiento del contexto original.
        contexto.ChangeTracker.Clear();

        var pagoRecuperado =
            await repositorio.ObtenerPorIdAsync(pagoId);

        // Assert
        Assert.NotNull(pagoRecuperado);

        Assert.True(pagoRecuperado.Id > 0);
        Assert.Equal(15000m, pagoRecuperado.Importe);
        Assert.Equal(FormaPago.Efectivo, pagoRecuperado.FormaPago);
        Assert.Equal(EstadoPago.Registrado, pagoRecuperado.Estado);
        Assert.Equal(usuario.Id, pagoRecuperado.UsuarioId);
        Assert.Equal(15000m, pagoRecuperado.ImporteDisponible);
        Assert.Empty(pagoRecuperado.Aplicaciones);

        Assert.Equal(
            "Pago de prueba de integración",
            pagoRecuperado.Observaciones);

        // No hacemos Commit.
        // Al finalizar, la transacción se revierte.
    }


    [Fact]
    public async Task AgregarAplicacion_DebePersistirYRecuperarAplicacion()
    {
        // Arrange
        await using var aplicacion =
            new SistemaVentaWebApplicationFactory();

        using var scope =
            aplicacion.Services.CreateScope();

        var contexto = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        await using var transaccion =
            await contexto.Database.BeginTransactionAsync();

        // Cliente de prueba.
        var identificador = Guid.NewGuid().ToString("N");
        var dni = Random.Shared.Next(10_000_000, 100_000_000).ToString();

        var cliente = new ClienteFisico(
            "Cliente",
            "Prueba",
            dni,
            "3815551234",
            $"cliente_{identificador}@test.com",
            "Dirección de prueba");

        contexto.Clientes.Add(cliente);
        await contexto.SaveChangesAsync();

        // Venta asociada al cliente.
        var venta = new Venta(cliente.Id);

        contexto.Ventas.Add(venta);
        await contexto.SaveChangesAsync();

        // Factura asociada a la venta.
        var factura = new Factura(
            venta.Id,
            TipoFactura.A,
            50000m,
            $"FAC-TEST-{identificador[..12]}");

        contexto.DocumentosComerciales.Add(factura);
        await contexto.SaveChangesAsync();

        // Usuario responsable del pago.
        var usuario = new Usuario(
            $"test_{identificador[..20]}",
            "Vendedor de prueba",
            "hash-de-prueba-no-utilizado-para-login",
            RolUsuario.Vendedor);

        contexto.Usuarios.Add(usuario);
        await contexto.SaveChangesAsync();

        // Pago de $30.000.
        var pago = new Pago(
            30000m,
            FormaPago.Efectivo,
            usuario.Id,
            "Pago con aplicación parcial");

        // Aplicamos $20.000 a la factura.
        pago.AgregarAplicacion(
            factura.Id,
            20000m);

        var repositorio = new PagoRepository(contexto);

        // Act
        await repositorio.AgregarAsync(pago);
        await contexto.SaveChangesAsync();

        var pagoId = pago.Id;
        var facturaId = factura.Id;

        // Forzamos la lectura desde PostgreSQL.
        contexto.ChangeTracker.Clear();

        var pagoRecuperado =
            await repositorio.ObtenerPorIdAsync(pagoId);

        // Assert
        Assert.NotNull(pagoRecuperado);

        Assert.Equal(30000m, pagoRecuperado.Importe);
        Assert.Equal(10000m, pagoRecuperado.ImporteDisponible);

        var aplicacionRecuperada =
            Assert.Single(pagoRecuperado.Aplicaciones);

        Assert.True(aplicacionRecuperada.Id > 0);

        Assert.Equal(
            facturaId,
            aplicacionRecuperada.DocumentoComercialId);

        Assert.Equal(
            pagoId,
            aplicacionRecuperada.PagoId);

        Assert.Equal(
            20000m,
            aplicacionRecuperada.ImporteAplicado);

        // Sin Commit: la transacción se revierte.
        // Verificamos el repositorio de documentos comerciales.
        var documentoRepository =
            new DocumentoComercialRepository(contexto);

        var documentoRecuperado =
            await documentoRepository.ObtenerPorIdAsync(facturaId);

        Assert.NotNull(documentoRecuperado);

        var facturaRecuperada =
            Assert.IsType<Factura>(documentoRecuperado);

        Assert.Equal(50000m, facturaRecuperada.ImporteTotal);

        // Verificamos el total aplicado a la factura.
        var totalAplicado =
            await documentoRepository.ObtenerTotalAplicadoAsync(
                facturaId);

        Assert.Equal(20000m, totalAplicado);

    }

    [Fact]
    public async Task AgregarAplicacionVenta_DebePersistirYActualizarImporteDisponible()
    {
        // Arrange
        await using var aplicacion =
            new SistemaVentaWebApplicationFactory();

        using var scope =
            aplicacion.Services.CreateScope();

        var contexto = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        await using var transaccion =
            await contexto.Database.BeginTransactionAsync();

        var identificador = Guid.NewGuid().ToString("N");

        // Crear cliente.
        var cliente = new ClienteFisico(
            "Cliente",
            "Prueba",
            Random.Shared.Next(10_000_000, 100_000_000).ToString(),
            "3815551234",
            $"cliente_{identificador}@test.com",
            "Dirección de prueba");

        contexto.Clientes.Add(cliente);
        await contexto.SaveChangesAsync();

        // Crear venta.
        var venta = new Venta(cliente.Id);

        contexto.Ventas.Add(venta);
        await contexto.SaveChangesAsync();

        // Crear usuario responsable del pago.
        var usuario = new Usuario(
            $"test_{identificador[..20]}",
            "Vendedor de prueba",
            "hash-de-prueba-no-utilizado-para-login",
            RolUsuario.Vendedor);

        contexto.Usuarios.Add(usuario);
        await contexto.SaveChangesAsync();

        // Registrar pago de $50.000.
        var pago = new Pago(
            50000m,
            FormaPago.Efectivo,
            usuario.Id,
            "Pago aplicado a venta");

        // Aplicar $20.000 a la venta.
        pago.AgregarAplicacionVenta(
            venta,
            20000m);

        var repositorio = new PagoRepository(contexto);

        // Act
        await repositorio.AgregarAsync(pago);
        await contexto.SaveChangesAsync();

        var pagoId = pago.Id;
        var ventaId = venta.Id;

        // Limpiar el seguimiento de EF Core para obligar
        // a recuperar los datos desde PostgreSQL.
        contexto.ChangeTracker.Clear();

        var pagoRecuperado =
            await repositorio.ObtenerPorIdAsync(pagoId);

        // Assert
        Assert.NotNull(pagoRecuperado);

        Assert.Equal(50000m, pagoRecuperado.Importe);

        Assert.Equal(
            30000m,
            pagoRecuperado.ImporteDisponible);

        Assert.Empty(pagoRecuperado.Aplicaciones);

        var aplicacionVenta =
            Assert.Single(pagoRecuperado.AplicacionesVentas);

        Assert.True(aplicacionVenta.Id > 0);

        Assert.Equal(
            pagoId,
            aplicacionVenta.PagoId);

        Assert.Equal(
            ventaId,
            aplicacionVenta.VentaId);

        Assert.Equal(
            20000m,
            aplicacionVenta.ImporteAplicado);

        // No hacemos Commit.
        // La transacción se revierte al finalizar.
    }



}
