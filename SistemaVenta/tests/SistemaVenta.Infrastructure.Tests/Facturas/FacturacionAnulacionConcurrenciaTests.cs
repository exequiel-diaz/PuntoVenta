
using Microsoft.EntityFrameworkCore;
using SistemaVenta.Application.UseCases.Facturas;
using SistemaVenta.Application.UseCases.Ventas;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;
using SistemaVenta.Infrastructure.Persistence;
using SistemaVenta.Infrastructure.Persistence.Repositories;
using SistemaVenta.Infrastructure.Repositories;
using SistemaVenta.Infrastructure.Tests.Persistence;
using Xunit;
using Npgsql;

namespace SistemaVenta.Infrastructure.Tests.Facturas
{
    public class FacturacionAnulacionConcurrenciaTests
    {
        private readonly TestDbContextFactory _factory = new();

        [Fact]
        public async Task FacturarPrimero_ImpideAnularLaVenta()
        {
            int ventaId = 0;
            int clienteId = 0;
            int productoId = 0;
            int categoriaId = 0;

            // ARRANGE: preparar datos persistidos.
            await using (var preparacion = _factory.CreateDbContext([]))
            {
                var cliente = new ClienteFisico(
                    nombre: "Cliente",
                    apellido: "ConcurrenciaFactura",
                    dni: Random.Shared.Next(
                        10_000_000, 100_000_000).ToString(),
                    telefono: "3811234567",
                    email: $"concurrencia-{Guid.NewGuid():N}@test.com",
                    direccion: "Tucumán");

                var categoria = new Categoria(
                    $"Concurrencia-{Guid.NewGuid():N}");

                var producto = new Producto(
                    codigoInterno: $"CFAC-{Guid.NewGuid():N}",
                    nombre: "Memoria RAM",
                    precioVenta: 15000m,
                    costo: 10000m,
                    stockMinimo: 1m,
                    categoria: categoria);

                producto.AumentarStock(10m);

                preparacion.Clientes.Add(cliente);
                preparacion.Productos.Add(producto);

                await preparacion.SaveChangesAsync();

                clienteId = cliente.Id;
                productoId = producto.Id;
                categoriaId = categoria.Id;

                var venta = new Venta(clienteId);

                venta.AgregarDetalle(
                    producto,
                    cantidad: 2m,
                    precioUnitario: 15000m);

                venta.Confirmar();

                preparacion.Ventas.Add(venta);

                await preparacion.SaveChangesAsync();

                ventaId = venta.Id;
            }

            try
            {
                // Dos contextos independientes.
                await using var contextoFactura =
                    _factory.CreateDbContext([]);

                await using var contextoAnulacion =
                    _factory.CreateDbContext([]);

                var emitirFactura = new EmitirFactura(
                    new VentaRepository(contextoFactura),
                    new DocumentoComercialRepository(contextoFactura),
                    contextoFactura,
                    new TransaccionVentaPostgres(contextoFactura));

                var anularVenta = new AnularVenta(
                    new VentaRepository(contextoAnulacion),
                    new ProductoRepository(contextoAnulacion),
                    new MovimientoStockRepository(contextoAnulacion),
                    contextoAnulacion,
                    new DocumentoComercialRepository(contextoAnulacion),
                    new TransaccionVentaPostgres(contextoAnulacion),
                    new CuentaCorrienteRepository(contextoAnulacion));

                // ACT: facturar primero.
                var facturaId = await emitirFactura.EjecutarAsync(
                    ventaId,
                    TipoFactura.B);

                // La anulación debe ser rechazada.
                var excepcion =
                    await Assert.ThrowsAsync<InvalidOperationException>(
                        () => anularVenta.EjecutarAsync(ventaId));

                Assert.Equal(
                    "No se puede anular una venta que ya tiene una factura registrada.",
                    excepcion.Message);

                // ASSERT: verificar desde un tercer contexto.
                await using var verificacion =
                    _factory.CreateDbContext([]);

                var ventaGuardada = await verificacion.Ventas
                    .AsNoTracking()
                    .SingleAsync(v => v.Id == ventaId);

                var facturaGuardada = await verificacion
                    .DocumentosComerciales
                    .OfType<Factura>()
                    .AsNoTracking()
                    .SingleAsync(f => f.VentaId == ventaId);

                var stockFinal = await verificacion.Productos
                    .Where(p => p.Id == productoId)
                    .Select(p => p.StockActual)
                    .SingleAsync();

                Assert.Equal(
                    EstadoVenta.Confirmada,
                    ventaGuardada.Estado);

                Assert.Equal(facturaId, facturaGuardada.Id);

                Assert.Equal(30000m, facturaGuardada.ImporteTotal);

                Assert.Equal(10m, stockFinal);
            }
            finally
            {
                // Limpieza en orden de dependencias.
                await using var limpieza =
                    _factory.CreateDbContext([]);

                if (ventaId > 0)
                {
                    await limpieza.DocumentosComerciales
                        .OfType<Factura>()
                        .Where(f => f.VentaId == ventaId)
                        .ExecuteDeleteAsync();

                    await limpieza.DetallesVenta
                        .Where(d => d.VentaId == ventaId)
                        .ExecuteDeleteAsync();

                    await limpieza.Ventas
                        .Where(v => v.Id == ventaId)
                        .ExecuteDeleteAsync();
                }

                if (productoId > 0)
                {
                    await limpieza.MovimientosStock
                        .Where(m => m.Producto.Id == productoId)
                        .ExecuteDeleteAsync();

                    await limpieza.Productos
                        .Where(p => p.Id == productoId)
                        .ExecuteDeleteAsync();
                }

                if (categoriaId > 0)
                {
                    await limpieza.Categorias
                        .Where(c => c.Id == categoriaId)
                        .ExecuteDeleteAsync();
                }

                if (clienteId > 0)
                {
                    await limpieza.Clientes
                        .Where(c => c.Id == clienteId)
                        .ExecuteDeleteAsync();
                }
            }
        }


        [Fact]
        public async Task DosTransacciones_NoPuedenBloquearLaMismaVentaAlMismoTiempo()
        {
            int ventaId = 0;
            int clienteId = 0;
            int productoId = 0;
            int categoriaId = 0;

            // ARRANGE: preparar una venta confirmada.
            await using (var preparacion = _factory.CreateDbContext([]))
            {
                var cliente = new ClienteFisico(
                    nombre: "Cliente",
                    apellido: "BloqueoConcurrente",
                    dni: Random.Shared.Next(
                        10_000_000, 100_000_000).ToString(),
                    telefono: "3811234567",
                    email: $"bloqueo-{Guid.NewGuid():N}@test.com",
                    direccion: "Tucumán");

                var categoria = new Categoria(
                    $"Bloqueo-{Guid.NewGuid():N}");

                var producto = new Producto(
                    codigoInterno: $"BLOQ-{Guid.NewGuid():N}",
                    nombre: "Producto bloqueo",
                    precioVenta: 15000m,
                    costo: 10000m,
                    stockMinimo: 1m,
                    categoria: categoria);

                preparacion.Clientes.Add(cliente);
                preparacion.Productos.Add(producto);

                await preparacion.SaveChangesAsync();

                clienteId = cliente.Id;
                productoId = producto.Id;
                categoriaId = categoria.Id;

                var venta = new Venta(clienteId);
                venta.AgregarDetalle(producto, 1m, 15000m);
                venta.Confirmar();

                preparacion.Ventas.Add(venta);
                await preparacion.SaveChangesAsync();

                ventaId = venta.Id;
            }

            try
            {
                await using var contextoA = _factory.CreateDbContext([]);
                await using var contextoB = _factory.CreateDbContext([]);

                // A inicia una transacción y bloquea la venta.
                await using var transaccionA =
                    await contextoA.Database.BeginTransactionAsync();

                await contextoA.Database.ExecuteSqlInterpolatedAsync(
                    $"""
            SELECT 1
            FROM "Ventas"
            WHERE "Id" = {ventaId}
            FOR UPDATE
            """);

                // B inicia otra transacción independiente.
                await using var transaccionB =
                    await contextoB.Database.BeginTransactionAsync();

                // Esperar como máximo 500 ms para adquirir el bloqueo.
                await contextoB.Database.ExecuteSqlRawAsync(
                    "SET LOCAL lock_timeout = '500ms'");

                // ACT: B intenta bloquear la misma venta.
                var excepcion = await Assert.ThrowsAsync<
                    PostgresException>(
                    async () =>
                    {
                        await contextoB.Database.ExecuteSqlInterpolatedAsync(
                            $"""
                    SELECT 1
                    FROM "Ventas"
                    WHERE "Id" = {ventaId}
                    FOR UPDATE
                    """);
                    });

                // ASSERT: PostgreSQL informa que no pudo
                // adquirir el bloqueo dentro del plazo.
                Assert.Equal("55P03", excepcion.SqlState);

                // Liberar ambas transacciones sin guardar cambios.
                await transaccionB.RollbackAsync();
                await transaccionA.RollbackAsync();
            }
            finally
            {
                // Limpieza de todos los datos de prueba.
                await using var limpieza = _factory.CreateDbContext([]);

                if (ventaId > 0)
                {
                    await limpieza.DetallesVenta
                        .Where(d => d.VentaId == ventaId)
                        .ExecuteDeleteAsync();

                    await limpieza.Ventas
                        .Where(v => v.Id == ventaId)
                        .ExecuteDeleteAsync();
                }

                if (productoId > 0)
                {
                    await limpieza.MovimientosStock
                        .Where(m => m.Producto.Id == productoId)
                        .ExecuteDeleteAsync();

                    await limpieza.Productos
                        .Where(p => p.Id == productoId)
                        .ExecuteDeleteAsync();
                }

                if (categoriaId > 0)
                {
                    await limpieza.Categorias
                        .Where(c => c.Id == categoriaId)
                        .ExecuteDeleteAsync();
                }

                if (clienteId > 0)
                {
                    await limpieza.Clientes
                        .Where(c => c.Id == clienteId)
                        .ExecuteDeleteAsync();
                }
            }
        }



    }
}
