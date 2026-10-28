
using Microsoft.EntityFrameworkCore;
using SistemaVenta.Application.DTOs.Ventas;
using SistemaVenta.Application.UseCases.Ventas;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;
using SistemaVenta.Infrastructure.Persistence;
using SistemaVenta.Infrastructure.Persistence.Repositories;
using SistemaVenta.Infrastructure.Repositories;
using SistemaVenta.Infrastructure.Tests.Persistence;
using Xunit;

namespace SistemaVenta.Infrastructure.Tests.Ventas
{
    public class AnularVentaIntegrationTests
    {
        private readonly TestDbContextFactory _factory = new();

        [Fact]
        public async Task AnularVenta_DebePersistirEstadoStockYMovimiento()
        {
            await using var context =
                _factory.CreateDbContext([]);

            // Los datos de la prueba se revertirán al finalizar.
            await using var transaccion =
                await context.Database.BeginTransactionAsync();

            // ARRANGE: cliente, categoría y producto.
            var cliente = new ClienteFisico(
                nombre: "Cliente",
                apellido: "Anulacion",
                dni: Random.Shared.Next(
                    10_000_000, 100_000_000).ToString(),
                telefono: "3811234567",
                email: $"anulacion-{Guid.NewGuid():N}@test.com",
                direccion: "Tucumán");

            var categoria = new Categoria(
                $"Anulacion-{Guid.NewGuid():N}",
                "Categoría de prueba");

            var producto = new Producto(
                codigoInterno: $"ANU-{Guid.NewGuid():N}",
                nombre: "Producto anulación",
                precioVenta: 10000m,
                costo: 5000m,
                stockMinimo: 1m,
                categoria: categoria);

            producto.AumentarStock(10m);

            context.Clientes.Add(cliente);
            context.Productos.Add(producto);

            await context.SaveChangesAsync();

            // Registrar una venta real mediante Application.
            var registrarVenta = new RegistrarVenta(
                new VentaRepository(context),
                new ClienteRepository(context),
                new ProductoRepository(context),
                new MovimientoStockRepository(context),
                context,
                new CuentaCorrienteRepository(context));

            await registrarVenta.EjecutarAsync(
                new RegistrarVentaDto
                {
                    ClienteId = cliente.Id,
                    Detalles = new()
                    {
                        new()
                        {
                            ProductoId = producto.Id,
                            Cantidad = 1.5m
                        }
                    }
                });

            var ventaId = await context.Ventas
                .Where(v => v.ClienteId == cliente.Id)
                .Select(v => v.Id)
                .SingleAsync();

            // Comprobar el estado antes de anular.
            context.ChangeTracker.Clear();

            var stockAntes = await context.Productos
                .Where(p => p.Id == producto.Id)
                .Select(p => p.StockActual)
                .SingleAsync();

            Assert.Equal(8.5m, stockAntes);

            // ACT: anular mediante el caso de uso real.

            var anularVenta = new AnularVenta(
                new VentaRepository(context),
                new ProductoRepository(context),
                new MovimientoStockRepository(context),
                context,
                new DocumentoComercialRepository(context),
                new TransaccionVentaPostgres(context),
                new CuentaCorrienteRepository(context));


            await anularVenta.EjecutarAsync(ventaId);

            // Consultar nuevamente los datos persistidos.
            context.ChangeTracker.Clear();

            var ventaGuardada = await context.Ventas
                .AsNoTracking()
                .SingleAsync(v => v.Id == ventaId);

            var productoGuardado = await context.Productos
                .AsNoTracking()
                .SingleAsync(p => p.Id == producto.Id);

            var movimientos = await context.MovimientosStock
                .AsNoTracking()
                .Where(m => m.Producto.Id == producto.Id)
                .ToListAsync();

            // ASSERT: estado y stock.
            Assert.Equal(
                EstadoVenta.Anulada,
                ventaGuardada.Estado);

            Assert.Equal(10m, productoGuardado.StockActual);

            // Deben existir dos movimientos:
            // uno de venta y otro de anulación.
            Assert.Equal(2, movimientos.Count);

            var anulacion = Assert.Single(
                movimientos,
                m => m.Tipo == TipoMovimientoStock.AnulacionVenta);

            Assert.Equal(1.5m, anulacion.Cantidad);
            Assert.Equal(8.5m, anulacion.StockAnterior);
            Assert.Equal(10m, anulacion.StockPosterior);

            // Sin Commit: la transacción se revierte
            // al finalizar la prueba.
        }


        [Fact]
        public async Task AnularVenta_DosOperacionesConcurrentes_NoDevuelveStockDosVeces()
        {
            int clienteId = 0;
            int categoriaId = 0;
            int productoId = 0;
            int ventaId = 0;

            // ARRANGE: crear una venta confirmada en PostgreSQL.
            await using (var preparacion = _factory.CreateDbContext([]))
            {
                var cliente = new ClienteFisico(
                    nombre: "Cliente",
                    apellido: "AnulacionConcurrente",
                    dni: Random.Shared.Next(10_000_000, 100_000_000).ToString(),
                    telefono: "3811234567",
                    email: $"anulacion-concurrente-{Guid.NewGuid():N}@test.com",
                    direccion: "Tucumán");

                var categoria = new Categoria(
                    $"Anulacion-{Guid.NewGuid():N}",
                    "Prueba de concurrencia");

                var producto = new Producto(
                    codigoInterno: $"ACON-{Guid.NewGuid():N}",
                    nombre: "Producto anulación concurrente",
                    precioVenta: 10000m,
                    costo: 5000m,
                    stockMinimo: 1m,
                    categoria: categoria);

                producto.AumentarStock(10m);

                preparacion.Clientes.Add(cliente);
                preparacion.Productos.Add(producto);

                await preparacion.SaveChangesAsync();

                clienteId = cliente.Id;
                categoriaId = categoria.Id;
                productoId = producto.Id;

                var registrarVenta = new RegistrarVenta(
                    new VentaRepository(preparacion),
                    new ClienteRepository(preparacion),
                    new ProductoRepository(preparacion),
                    new MovimientoStockRepository(preparacion),
                    preparacion,
                    new CuentaCorrienteRepository(preparacion));

                await registrarVenta.EjecutarAsync(
                    new RegistrarVentaDto
                    {
                        ClienteId = clienteId,
                        Detalles = new()
                        {
                    new()
                    {
                        ProductoId = productoId,
                        Cantidad = 3m
                    }
                        }
                    });

                ventaId = await preparacion.Ventas
                    .Where(v => v.ClienteId == clienteId)
                    .Select(v => v.Id)
                    .SingleAsync();
            }

            try
            {
                // Dos contextos independientes.
                await using var contextoA = _factory.CreateDbContext([]);
                await using var contextoB = _factory.CreateDbContext([]);

                // Ambos leen la misma venta antes de anularla.
                var ventaA = await contextoA.Ventas
                    .Include(v => v.Detalles)
                    .SingleAsync(v => v.Id == ventaId);

                var ventaB = await contextoB.Ventas
                    .Include(v => v.Detalles)
                    .SingleAsync(v => v.Id == ventaId);

                Assert.Equal(EstadoVenta.Confirmada, ventaA.Estado);
                Assert.Equal(EstadoVenta.Confirmada, ventaB.Estado);


                var anularA = new AnularVenta(
                    new VentaRepository(contextoA),
                    new ProductoRepository(contextoA),
                    new MovimientoStockRepository(contextoA),
                    contextoA,
                    new DocumentoComercialRepository(contextoA),
                    new TransaccionVentaPostgres(contextoA),
                    new CuentaCorrienteRepository(contextoA));



                var anularB = new AnularVenta(
                    new VentaRepository(contextoB),
                    new ProductoRepository(contextoB),
                    new MovimientoStockRepository(contextoB),
                    contextoB,
                    new DocumentoComercialRepository(contextoB),
                    new TransaccionVentaPostgres(contextoB),
                    new CuentaCorrienteRepository(contextoB));


                // ACT: A anula la venta.
                await anularA.EjecutarAsync(ventaId);


                // B había cargado una versión anterior de la venta.
                // Al obtener el bloqueo, se actualiza su estado.
                // La segunda anulación debe rechazarse.

                var excepcion = await Assert.ThrowsAsync<InvalidOperationException>(
                    () => anularB.EjecutarAsync(ventaId));

                Assert.Equal(
                    "Solo se puede anular una venta confirmada.",
                    excepcion.Message);


                // ASSERT: verificar desde un tercer contexto.
                await using var verificacion = _factory.CreateDbContext([]);

                var ventaGuardada = await verificacion.Ventas
                    .AsNoTracking()
                    .SingleAsync(v => v.Id == ventaId);

                var productoGuardado = await verificacion.Productos
                    .AsNoTracking()
                    .SingleAsync(p => p.Id == productoId);

                var movimientos = await verificacion.MovimientosStock
                    .AsNoTracking()
                    .Where(m => m.Producto.Id == productoId)
                    .ToListAsync();

                Assert.Equal(EstadoVenta.Anulada, ventaGuardada.Estado);

                // Stock inicial: 10
                // Venta: -3
                // Una sola anulación: +3
                Assert.Equal(10m, productoGuardado.StockActual);

                // Un movimiento de venta y uno de anulación.
                Assert.Equal(2, movimientos.Count);

                Assert.Single(
                    movimientos,
                    m => m.Tipo == TipoMovimientoStock.AnulacionVenta);

                Assert.Single(
                    movimientos,
                    m => m.Tipo == TipoMovimientoStock.Venta);
            }
            finally
            {
                // Limpieza respetando las relaciones entre tablas.
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


        [Fact]
        public async Task AnularVentaACredito_DebeRestaurarStockYRevertirDeuda()
        {
            await using var context = _factory.CreateDbContext([]);
            await using var transaccion =
                await context.Database.BeginTransactionAsync();

            // ARRANGE: cliente, producto y cuenta corriente.
            var cliente = new ClienteFisico(
                nombre: "Cliente",
                apellido: "AnulacionCredito",
                dni: Random.Shared.Next(10_000_000, 100_000_000).ToString(),
                telefono: "3811234567",
                email: $"anulacion-credito-{Guid.NewGuid():N}@test.com",
                direccion: "Tucumán");

            var categoria = new Categoria(
                $"Credito-{Guid.NewGuid():N}",
                "Categoría de prueba");

            var producto = new Producto(
                codigoInterno: $"ACR-{Guid.NewGuid():N}",
                nombre: "Producto venta a crédito",
                precioVenta: 25000m,
                costo: 15000m,
                stockMinimo: 1m,
                categoria: categoria);

            producto.AumentarStock(10m);

            context.Clientes.Add(cliente);
            context.Productos.Add(producto);

            await context.SaveChangesAsync();

            var cuenta = new CuentaCorriente(
                clienteId: cliente.Id,
                limiteDeuda: 125000m);

            context.CuentasCorrientes.Add(cuenta);
            await context.SaveChangesAsync();

            // Registrar una venta a crédito por $50.000.
            var registrarVenta = new RegistrarVenta(
                new VentaRepository(context),
                new ClienteRepository(context),
                new ProductoRepository(context),
                new MovimientoStockRepository(context),
                context,
                new CuentaCorrienteRepository(context));

            await registrarVenta.EjecutarAsync(
                new RegistrarVentaDto
                {
                    ClienteId = cliente.Id,
                    Modalidad = ModalidadVenta.CuentaCorriente,
                    Detalles = new()
                    {
                new()
                {
                    ProductoId = producto.Id,
                    Cantidad = 2m
                }
                    }
                });

            var ventaId = await context.Ventas
                .Where(v => v.ClienteId == cliente.Id)
                .Select(v => v.Id)
                .SingleAsync();

            // Comprobar la situación anterior a la anulación.
            context.ChangeTracker.Clear();

            var cuentaAntes = await context.CuentasCorrientes
                .Include(c => c.Movimientos)
                .SingleAsync(c => c.ClienteId == cliente.Id);

            Assert.Equal(50000m, cuentaAntes.SaldoActual);

            var stockAntes = await context.Productos
                .Where(p => p.Id == producto.Id)
                .Select(p => p.StockActual)
                .SingleAsync();

            Assert.Equal(8m, stockAntes);

            context.ChangeTracker.Clear();

            // ACT: anular la venta.
            var anularVenta = new AnularVenta(
                new VentaRepository(context),
                new ProductoRepository(context),
                new MovimientoStockRepository(context),
                context,
                new DocumentoComercialRepository(context),
                new TransaccionVentaPostgres(context),
                new CuentaCorrienteRepository(context));

            await anularVenta.EjecutarAsync(ventaId);

            // ASSERT: consultar nuevamente PostgreSQL.
            context.ChangeTracker.Clear();

            var ventaGuardada = await context.Ventas
                .AsNoTracking()
                .SingleAsync(v => v.Id == ventaId);

            var productoGuardado = await context.Productos
                .AsNoTracking()
                .SingleAsync(p => p.Id == producto.Id);

            var cuentaGuardada = await context.CuentasCorrientes
                .AsNoTracking()
                .Include(c => c.Movimientos)
                .SingleAsync(c => c.ClienteId == cliente.Id);

            Assert.Equal(EstadoVenta.Anulada, ventaGuardada.Estado);
            Assert.Equal(10m, productoGuardado.StockActual);
            Assert.Equal(0m, cuentaGuardada.SaldoActual);
            Assert.Equal(125000m,
                cuentaGuardada.LimiteDeuda - cuentaGuardada.SaldoActual);

            // El débito original permanece registrado.
            var debito = Assert.Single(
                cuentaGuardada.Movimientos,
                m => m.EsDebito());

            Assert.Equal(50000m, debito.Importe);
            Assert.Equal(ventaId, debito.VentaId);

            // La anulación genera un crédito compensatorio.
            var credito = Assert.Single(
                cuentaGuardada.Movimientos,
                m => !m.EsDebito());

            Assert.Equal(50000m, credito.Importe);
            Assert.Equal(ventaId, credito.VentaId);

            Assert.Equal(2, cuentaGuardada.Movimientos.Count);

            // Sin Commit: los datos se revierten al finalizar.
        }


        [Fact]
        public async Task AnularVentaACredito_ConPagoParcial_NoDebeAnular()
        {
            await using var context = _factory.CreateDbContext([]);
            await using var transaccion =
                await context.Database.BeginTransactionAsync();

            var cliente = new ClienteFisico(
                nombre: "Cliente",
                apellido: "PagoParcial",
                dni: Random.Shared.Next(10_000_000, 100_000_000).ToString(),
                telefono: "3811234567",
                email: $"pago-parcial-{Guid.NewGuid():N}@test.com",
                direccion: "Tucumán");

            var categoria = new Categoria(
                $"PagoParcial-{Guid.NewGuid():N}");

            var producto = new Producto(
                codigoInterno: $"PPA-{Guid.NewGuid():N}",
                nombre: "Producto pago parcial",
                precioVenta: 25000m,
                costo: 15000m,
                stockMinimo: 1m,
                categoria: categoria);

            producto.AumentarStock(10m);

            context.Clientes.Add(cliente);
            context.Productos.Add(producto);
            await context.SaveChangesAsync();

            var cuenta = new CuentaCorriente(cliente.Id, 125000m);
            context.CuentasCorrientes.Add(cuenta);
            await context.SaveChangesAsync();

            var registrar = new RegistrarVenta(
                new VentaRepository(context),
                new ClienteRepository(context),
                new ProductoRepository(context),
                new MovimientoStockRepository(context),
                context,
                new CuentaCorrienteRepository(context));

            await registrar.EjecutarAsync(new RegistrarVentaDto
            {
                ClienteId = cliente.Id,
                Modalidad = ModalidadVenta.CuentaCorriente,
                Detalles = new()
        {
            new()
            {
                ProductoId = producto.Id,
                Cantidad = 2m
            }
        }
            });

            var ventaId = await context.Ventas
                .Where(v => v.ClienteId == cliente.Id)
                .Select(v => v.Id)
                .SingleAsync();

            // Simular un pago parcial de $20.000.
            cuenta.RegistrarCredito(20000m, "Pago parcial del cliente");
            await context.SaveChangesAsync();

            context.ChangeTracker.Clear();

            var anular = new AnularVenta(
                new VentaRepository(context),
                new ProductoRepository(context),
                new MovimientoStockRepository(context),
                context,
                new DocumentoComercialRepository(context),
                new TransaccionVentaPostgres(context),
                new CuentaCorrienteRepository(context));

            // La anulación debe ser rechazada.
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => anular.EjecutarAsync(ventaId));

            context.ChangeTracker.Clear();

            var ventaGuardada = await context.Ventas
                .AsNoTracking()
                .SingleAsync(v => v.Id == ventaId);

            var productoGuardado = await context.Productos
                .AsNoTracking()
                .SingleAsync(p => p.Id == producto.Id);

            var cuentaGuardada = await context.CuentasCorrientes
                .AsNoTracking()
                .Include(c => c.Movimientos)
                .SingleAsync(c => c.ClienteId == cliente.Id);

            Assert.Equal(EstadoVenta.Confirmada, ventaGuardada.Estado);
            Assert.Equal(8m, productoGuardado.StockActual);
            Assert.Equal(30000m, cuentaGuardada.SaldoActual);
            Assert.Equal(2, cuentaGuardada.Movimientos.Count);
        }






    }
}
