
using Microsoft.EntityFrameworkCore;
using SistemaVenta.Application.DTOs.Ventas;
using SistemaVenta.Application.UseCases.Ventas;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;
using SistemaVenta.Infrastructure.Persistence;
using SistemaVenta.Infrastructure.Persistence.Repositories;
using SistemaVenta.Infrastructure.Tests.Persistence;
using Xunit;
using SistemaVenta.Infrastructure.Repositories;
using SistemaVenta.Application.Exceptions;

namespace SistemaVenta.Infrastructure.Tests.Ventas
{
    public class RegistrarVentaIntegrationTests
    {
        private readonly TestDbContextFactory _factory = new();

        [Fact]
        public async Task RegistrarVenta_DebePersistirVentaDetallesYStock()
        {
            // ARRANGE: base de datos exclusiva de pruebas.
            await using var context =
                _factory.CreateDbContext([]);

            // Todos los cambios de esta prueba se revertirán.
            await using var transaccion =
                await context.Database.BeginTransactionAsync();

            var cliente = new ClienteFisico(
                nombre: "Cliente",
                apellido: "VentaPrueba",
                dni: Random.Shared.Next(
                    10_000_000, 100_000_000).ToString(),
                telefono: "3811234567",
                email: $"venta-{Guid.NewGuid():N}@test.com",
                direccion: "Tucumán");

            var categoria = new Categoria(
                "Categoría de prueba",
                "Productos de integración");

            var producto1 = new Producto(
                codigoInterno: $"TEC-{Guid.NewGuid():N}",
                nombre: "Teclado de prueba",
                precioVenta: 10000m,
                costo: 5000m,
                stockMinimo: 1m,
                categoria: categoria);

            var producto2 = new Producto(
                codigoInterno: $"MOU-{Guid.NewGuid():N}",
                nombre: "Mouse de prueba",
                precioVenta: 5000m,
                costo: 2000m,
                stockMinimo: 1m,
                categoria: categoria);

            producto1.AumentarStock(10m);
            producto2.AumentarStock(8m);

            context.Clientes.Add(cliente);
            context.Productos.AddRange(producto1, producto2);

            await context.SaveChangesAsync();

            var ventaRepository =
                new VentaRepository(context);

            var clienteRepository =
                new ClienteRepository(context);

            var productoRepository =
                new ProductoRepository(context);

            var movimientoRepository =
                new MovimientoStockRepository(context);

            var registrarVenta = new RegistrarVenta(
                new VentaRepository(context),
                new ClienteRepository(context),
                new ProductoRepository(context),
                new MovimientoStockRepository(context),
                context,
                new CuentaCorrienteRepository(context));

            var solicitud = new RegistrarVentaDto
            {
                ClienteId = cliente.Id,
                Detalles = new()
                {
                    new()
                    {
                        ProductoId = producto1.Id,
                        Cantidad = 2m
                    },
                    new()
                    {
                        ProductoId = producto2.Id,
                        Cantidad = 3m
                    }
                }
            };

            // ACT
            await registrarVenta.EjecutarAsync(solicitud);

            // Limpiamos el seguimiento para consultar
            // los datos guardados realmente en PostgreSQL.
            context.ChangeTracker.Clear();

            var ventaGuardada = await context.Ventas
                .Include(v => v.Detalles)
                .SingleAsync(v => v.ClienteId == cliente.Id);

            var productosGuardados = await context.Productos
                .Where(p =>
                    p.Id == producto1.Id ||
                    p.Id == producto2.Id)
                .ToListAsync();

            var movimientos = await context.MovimientosStock
                .Where(m =>
                    m.Producto.Id == producto1.Id ||
                    m.Producto.Id == producto2.Id)
                .ToListAsync();

            // ASSERT: venta.
            Assert.True(ventaGuardada.Id > 0);
            Assert.Equal(
                EstadoVenta.Confirmada,
                ventaGuardada.Estado);

            Assert.Equal(35000m, ventaGuardada.Total);
            Assert.Equal(2, ventaGuardada.Detalles.Count);

            // ASSERT: inventario.
            var teclado = productosGuardados.Single(
                p => p.Id == producto1.Id);

            var mouse = productosGuardados.Single(
                p => p.Id == producto2.Id);

            Assert.Equal(8m, teclado.StockActual);
            Assert.Equal(5m, mouse.StockActual);

            // ASSERT: movimientos.
            Assert.Equal(2, movimientos.Count);

            Assert.All(movimientos, movimiento =>
                Assert.Equal(
                    TipoMovimientoStock.Venta,
                    movimiento.Tipo));

            // No hacemos Commit.
            // Al finalizar se revierte toda la prueba.
        }

        [Fact]
        public async Task GuardarVenta_SiFallaUnMovimiento_NoDebePersistirCambiosParciales()
        {
            // ARRANGE
            await using var context = _factory.CreateDbContext([]);

            await using var transaccion =
                await context.Database.BeginTransactionAsync();

            var cliente = new ClienteFisico(
                nombre: "Cliente",
                apellido: "Atomicidad",
                dni: Random.Shared.Next(10_000_000, 100_000_000).ToString(),
                telefono: "3811234567",
                email: $"atomicidad-{Guid.NewGuid():N}@test.com",
                direccion: "Tucumán");

            var categoria = new Categoria(
                "Categoría atomicidad",
                "Prueba de transacciones");

            var producto = new Producto(
                codigoInterno: $"ATM-{Guid.NewGuid():N}",
                nombre: "Producto atomicidad",
                precioVenta: 15000m,
                costo: 8000m,
                stockMinimo: 1m,
                categoria: categoria);

            producto.AumentarStock(10m);

            context.Clientes.Add(cliente);
            context.Productos.Add(producto);

            await context.SaveChangesAsync();

            int clienteId = cliente.Id;
            int productoId = producto.Id;

            // ACT: simulamos las modificaciones que realiza una venta.
            var venta = new Venta(clienteId);

            venta.AgregarDetalle(producto, 3m, 15000m);
            venta.Confirmar();

            producto.DisminuirStock(3m);

            var movimientoInvalido = new MovimientoStock(
                producto,
                TipoMovimientoStock.Venta,
                3m,
                10m,
                7m,
                new string('X', 501));

            context.Ventas.Add(venta);
            context.MovimientosStock.Add(movimientoInvalido);

            // PostgreSQL debe rechazar el motivo de 501 caracteres.
            await Assert.ThrowsAsync<DbUpdateException>(
                () => context.SaveChangesAsync());

            // La transacción queda en estado de error tras la excepción.
            // Revertimos explícitamente todos los cambios de la prueba.
            await transaccion.RollbackAsync();

            // ASSERT: verificamos el estado desde un contexto nuevo.
            await using var verificacion =
                _factory.CreateDbContext([]);

            var ventasPersistidas = await verificacion.Ventas
                .CountAsync(v => v.ClienteId == clienteId);

            Assert.Equal(0, ventasPersistidas);

            var productoPersistido =
                await verificacion.Productos
                    .FirstOrDefaultAsync(p => p.Id == productoId);

            // Los datos iniciales también se revierten porque pertenecen
            // a la transacción de la prueba.
            Assert.Null(productoPersistido);
        }

        [Fact]
        public async Task GuardarVenta_SiFallaUnMovimiento_NoDebeGuardarCambiosParciales()
        {
            // ARRANGE: preparar datos válidos.
            await using var context = _factory.CreateDbContext([]);

            await using var transaccion =
                await context.Database.BeginTransactionAsync();

            var cliente = new ClienteFisico(
                nombre: "Cliente",
                apellido: "Atomicidad",
                dni: Random.Shared.Next(10_000_000, 100_000_000).ToString(),
                telefono: "3811234567",
                email: $"atomicidad-{Guid.NewGuid():N}@test.com",
                direccion: "Tucumán");

            var categoria = new Categoria(
                "Categoría atomicidad",
                "Prueba de atomicidad");

            var producto = new Producto(
                codigoInterno: $"ATM-{Guid.NewGuid():N}",
                nombre: "Producto atomicidad",
                precioVenta: 15000m,
                costo: 8000m,
                stockMinimo: 1m,
                categoria: categoria);

            producto.AumentarStock(10m);

            context.Clientes.Add(cliente);
            context.Productos.Add(producto);

            await context.SaveChangesAsync();

            int clienteId = cliente.Id;
            int productoId = producto.Id;

            // ACT: preparar una venta y un movimiento inválido.
            var venta = new Venta(clienteId);

            venta.AgregarDetalle(producto, 3m, 15000m);
            venta.Confirmar();

            producto.DisminuirStock(3m);

            var movimientoInvalido = new MovimientoStock(
                producto,
                TipoMovimientoStock.Venta,
                3m,
                10m,
                7m,
                new string('X', 501));

            context.Ventas.Add(venta);
            context.MovimientosStock.Add(movimientoInvalido);

            // Un solo SaveChangesAsync intenta guardar todo.
            await Assert.ThrowsAsync<DbUpdateException>(
                () => context.SaveChangesAsync());

            // EF Core utiliza un savepoint dentro de la transacción
            // y revierte el SaveChangesAsync que falló.
            context.ChangeTracker.Clear();

            // ASSERT: los datos iniciales deben seguir existiendo.
            var productoRecuperado =
                await context.Productos.SingleAsync(
                    p => p.Id == productoId);

            Assert.Equal(10m, productoRecuperado.StockActual);

            // La venta no debe haberse guardado.
            var cantidadVentas =
                await context.Ventas.CountAsync(
                    v => v.ClienteId == clienteId);

            Assert.Equal(0, cantidadVentas);

            // Tampoco debe haberse guardado el movimiento inválido.
            var movimientos =
                await context.MovimientosStock.CountAsync(
                    m => m.Producto.Id == productoId);

            Assert.Equal(0, movimientos);

            // Se revierten también los datos iniciales al terminar.
            await transaccion.RollbackAsync();
        }

        [Fact]
        public async Task RegistrarVenta_SiHayConflictoConcurrencia_NoDebeGuardarVentaParcial()
        {
            int clienteId = 0;
            int categoriaId = 0;
            int productoId = 0;

            // ARRANGE: datos iniciales confirmados en PostgreSQL.
            await using (var preparacion = _factory.CreateDbContext([]))
            {
                var cliente = new ClienteFisico(
                    nombre: "Cliente",
                    apellido: "Concurrencia",
                    dni: Random.Shared.Next(
                        10_000_000, 100_000_000).ToString(),
                    telefono: "3811234567",
                    email: $"concurrencia-{Guid.NewGuid():N}@test.com",
                    direccion: "Tucumán");

                var categoria = new Categoria(
                    $"Concurrencia-{Guid.NewGuid():N}",
                    "Categoría para prueba de ventas");

                var producto = new Producto(
                    codigoInterno: $"VCON-{Guid.NewGuid():N}",
                    nombre: "Producto venta concurrente",
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
            }

            try
            {
                await using var contextoA =
                    _factory.CreateDbContext([]);

                await using var contextoB =
                    _factory.CreateDbContext([]);

                // Ambos vendedores leen el producto antes
                // de que cualquiera registre una venta.
                var productoA = await contextoA.Productos
                    .SingleAsync(p => p.Id == productoId);

                var productoB = await contextoB.Productos
                    .SingleAsync(p => p.Id == productoId);

                Assert.Equal(10m, productoA.StockActual);
                Assert.Equal(10m, productoB.StockActual);

                var registrarVentaA = new RegistrarVenta(
                    new VentaRepository(contextoA),
                    new ClienteRepository(contextoA),
                    new ProductoRepository(contextoA),
                    new MovimientoStockRepository(contextoA),
                    contextoA,
                    new CuentaCorrienteRepository(contextoA));

                var registrarVentaB = new RegistrarVenta(
                    new VentaRepository(contextoB),
                    new ClienteRepository(contextoB),
                    new ProductoRepository(contextoB),
                    new MovimientoStockRepository(contextoB),
                    contextoB,
                    new CuentaCorrienteRepository(contextoB));

                var solicitudA = new RegistrarVentaDto
                {
                    ClienteId = clienteId,
                    Detalles = new()
            {
                new()
                {
                    ProductoId = productoId,
                    Cantidad = 4m
                }
            }
                };

                var solicitudB = new RegistrarVentaDto
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
                };

                // ACT: el vendedor A confirma su venta.
                await registrarVentaA.EjecutarAsync(solicitudA);

                // El vendedor B conserva la versión anterior
                // del producto en su propio contexto.
                await Assert.ThrowsAsync<ConflictoConcurrenciaException>(
                    () => registrarVentaB.EjecutarAsync(solicitudB));

                // ASSERT: consultar desde un contexto nuevo.
                await using var verificacion =
                    _factory.CreateDbContext([]);

                var productoGuardado = await verificacion.Productos
                    .AsNoTracking()
                    .SingleAsync(p => p.Id == productoId);

                Assert.Equal(6m, productoGuardado.StockActual);

                var ventas = await verificacion.Ventas
                    .AsNoTracking()
                    .Include(v => v.Detalles)
                    .Where(v => v.ClienteId == clienteId)
                    .ToListAsync();

                // Solo debe existir la venta del vendedor A.
                var ventaGuardada = Assert.Single(ventas);

                Assert.Equal(
                    EstadoVenta.Confirmada,
                    ventaGuardada.Estado);

                var detalleGuardado =
                    Assert.Single(ventaGuardada.Detalles);

                Assert.Equal(4m, detalleGuardado.Cantidad);

                var movimientos = await verificacion.MovimientosStock
                    .AsNoTracking()
                    .Where(m => m.Producto.Id == productoId)
                    .ToListAsync();

                // Solo debe existir el movimiento de A.
                var movimientoGuardado =
                    Assert.Single(movimientos);

                Assert.Equal(
                    TipoMovimientoStock.Venta,
                    movimientoGuardado.Tipo);

                Assert.Equal(4m, movimientoGuardado.Cantidad);
            }
            finally
            {
                // Limpieza de los datos creados por la prueba.
                await using var limpieza =
                    _factory.CreateDbContext([]);

                if (clienteId > 0)
                {
                    var ventasIds = await limpieza.Ventas
                        .Where(v => v.ClienteId == clienteId)
                        .Select(v => v.Id)
                        .ToListAsync();

                    await limpieza.DetallesVenta
                        .Where(d => ventasIds.Contains(d.VentaId))
                        .ExecuteDeleteAsync();

                    await limpieza.Ventas
                        .Where(v => v.ClienteId == clienteId)
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
        public async Task RegistrarVentaACredito_DebeGuardarVentaYDebitoVinculados()
        {
            // ARRANGE
            await using var context = _factory.CreateDbContext([]);

            await using var transaccion =
                await context.Database.BeginTransactionAsync();

            var cliente = new ClienteFisico(
                nombre: "Cliente",
                apellido: "CreditoPrueba",
                dni: Random.Shared.Next(10_000_000, 100_000_000).ToString(),
                telefono: "3811234567",
                email: $"credito-{Guid.NewGuid():N}@test.com",
                direccion: "Tucumán");

            var categoria = new Categoria(
                "Categoría crédito",
                "Prueba de venta a cuenta corriente");

            var producto = new Producto(
                codigoInterno: $"CRED-{Guid.NewGuid():N}",
                nombre: "Memoria RAM de prueba",
                precioVenta: 25000m,
                costo: 15000m,
                stockMinimo: 2m,
                categoria: categoria);

            producto.AumentarStock(10m);

            context.Clientes.Add(cliente);
            context.Productos.Add(producto);

            await context.SaveChangesAsync();

            var cuenta = new CuentaCorriente(
                cliente.Id,
                125000m);

            context.CuentasCorrientes.Add(cuenta);

            await context.SaveChangesAsync();

            var registrarVenta = new RegistrarVenta(
                new VentaRepository(context),
                new ClienteRepository(context),
                new ProductoRepository(context),
                new MovimientoStockRepository(context),
                context,
                new CuentaCorrienteRepository(context));

            var solicitud = new RegistrarVentaDto
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
            };

            // ACT
            int ventaId = await registrarVenta.EjecutarAsync(solicitud);

            // Consultar los datos realmente guardados.
            context.ChangeTracker.Clear();

            var ventaGuardada = await context.Ventas
                .Include(v => v.Detalles)
                .SingleAsync(v => v.Id == ventaId);

            var cuentaGuardada = await context.CuentasCorrientes
                .Include(c => c.Movimientos)
                .SingleAsync(c => c.ClienteId == cliente.Id);

            var productoGuardado = await context.Productos
                .SingleAsync(p => p.Id == producto.Id);

            var debito = Assert.Single(
                cuentaGuardada.Movimientos);

            // ASSERT
            Assert.Equal(
                EstadoVenta.Confirmada,
                ventaGuardada.Estado);

            Assert.Equal(50000m, ventaGuardada.Total);

            Assert.Equal(8m, productoGuardado.StockActual);

            Assert.True(debito.EsDebito());

            Assert.Equal(50000m, debito.Importe);

            Assert.Equal(ventaId, debito.VentaId);

            Assert.Equal(50000m, cuentaGuardada.SaldoActual);

            Assert.Equal(
                75000m,
                cuentaGuardada.LimiteDeuda - cuentaGuardada.SaldoActual);

            // La transacción se revierte al finalizar.
        }


    }
}
