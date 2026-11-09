
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

            // Crear un vendedor válido para registrar la venta.
            var vendedor = new Usuario(
                nombreUsuario: $"vendedor_{Guid.NewGuid():N}",
                nombreCompleto: "Vendedor de prueba",
                passwordHash: "hash-de-prueba",
                rol: RolUsuario.Vendedor);

            // Guardar los datos iniciales.
            context.Clientes.Add(cliente);
            context.Productos.Add(producto);
            context.Usuarios.Add(vendedor);

            await context.SaveChangesAsync();

            // Registrar una venta real mediante Application.
            var registrarVenta = new RegistrarVenta(
                new VentaRepository(context),
                new ClienteRepository(context),
                new ProductoRepository(context),
                new MovimientoStockRepository(context),
                context,
                new CuentaCorrienteRepository(context),
                new UsuarioRepository(context));

            var solicitud = new RegistrarVentaDto
            {
                ClienteId = cliente.Id,
                Detalles = new()
        {
            new()
            {
                ProductoId = producto.Id,
                Cantidad = 2m
            }
        }
            };

            // Registrar la venta indicando el vendedor.
            int ventaId = await registrarVenta.EjecutarAsync(
                solicitud,
                vendedor.Id);

            // Comprobar el estado antes de anular.
            context.ChangeTracker.Clear();

            var stockAntes = await context.Productos
                .Where(p => p.Id == producto.Id)
                .Select(p => p.StockActual)
                .SingleAsync();

            Assert.Equal(8m, stockAntes);

            // ACT: anular mediante el caso de uso real.
            var anularVenta = new AnularVenta(
                new VentaRepository(context),
                new ProductoRepository(context),
                new MovimientoStockRepository(context),
                context,
                new DocumentoComercialRepository(context),
                new TransaccionVentaPostgres(context),
                new CuentaCorrienteRepository(context));

            await anularVenta.EjecutarAsync(ventaId, vendedor.Id);

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

            // ASSERT: verificar estado de la venta.
            Assert.Equal(
                EstadoVenta.Anulada,
                ventaGuardada.Estado);

            // Verificar que conserva el vendedor original.
            Assert.Equal(
                vendedor.Id,
                ventaGuardada.VendedorId);

            // El stock debe regresar a su cantidad inicial.
            Assert.Equal(
                10m,
                productoGuardado.StockActual);

            // Deben existir dos movimientos:
            // uno de venta y otro de anulación.
            Assert.Equal(2, movimientos.Count);

            var movimientoVenta = Assert.Single(
                movimientos,
                m => m.Tipo == TipoMovimientoStock.Venta);

            Assert.Equal(2m, movimientoVenta.Cantidad);

            var anulacion = Assert.Single(
                movimientos,
                m => m.Tipo == TipoMovimientoStock.AnulacionVenta);

            Assert.Equal(2m, anulacion.Cantidad);
            Assert.Equal(8m, anulacion.StockAnterior);
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
            int vendedorId = 0;

            // ARRANGE: crear una venta confirmada en PostgreSQL.
            await using (var preparacion = _factory.CreateDbContext([]))
            {
                var cliente = new ClienteFisico(
                    nombre: "Cliente",
                    apellido: "AnulacionConcurrente",
                    dni: Random.Shared.Next(
                        10_000_000, 100_000_000).ToString(),
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

                // Crear un vendedor activo y autorizado.
                var vendedor = new Usuario(
                    nombreUsuario: $"vendedor_{Guid.NewGuid():N}",
                    nombreCompleto: "Vendedor anulación concurrente",
                    passwordHash: "hash-de-prueba",
                    rol: RolUsuario.Vendedor);

                // Guardar los datos iniciales.
                preparacion.Clientes.Add(cliente);
                preparacion.Productos.Add(producto);
                preparacion.Usuarios.Add(vendedor);

                await preparacion.SaveChangesAsync();

                clienteId = cliente.Id;
                categoriaId = categoria.Id;
                productoId = producto.Id;
                vendedorId = vendedor.Id;

                // Registrar una venta real mediante Application.
                var registrarVenta = new RegistrarVenta(
                    new VentaRepository(preparacion),
                    new ClienteRepository(preparacion),
                    new ProductoRepository(preparacion),
                    new MovimientoStockRepository(preparacion),
                    preparacion,
                    new CuentaCorrienteRepository(preparacion),
                    new UsuarioRepository(preparacion));

                var solicitud = new RegistrarVentaDto
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

                // Registrar la venta indicando el vendedor.
                ventaId = await registrarVenta.EjecutarAsync(
                    solicitud,
                    vendedorId);
            }

            try
            {
                // Dos contextos independientes.
                await using var contextoA =
                    _factory.CreateDbContext([]);

                await using var contextoB =
                    _factory.CreateDbContext([]);

                // Ambos leen la misma venta antes de anularla.
                var ventaA = await contextoA.Ventas
                    .Include(v => v.Detalles)
                    .SingleAsync(v => v.Id == ventaId);

                var ventaB = await contextoB.Ventas
                    .Include(v => v.Detalles)
                    .SingleAsync(v => v.Id == ventaId);

                Assert.Equal(
                    EstadoVenta.Confirmada,
                    ventaA.Estado);

                Assert.Equal(
                    EstadoVenta.Confirmada,
                    ventaB.Estado);

                // Preparar los casos de uso de anulación.
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
                await anularA.EjecutarAsync(ventaId, vendedorId);

                // B había cargado una versión anterior de la venta.
                // La segunda anulación debe rechazarse.
                var excepcion =
                    await Assert.ThrowsAsync<InvalidOperationException>(
                        () => anularB.EjecutarAsync(ventaId, vendedorId));

                Assert.Equal(
                    "Solo se puede anular una venta confirmada.",
                    excepcion.Message);

                // ASSERT: verificar desde un tercer contexto.
                await using var verificacion =
                    _factory.CreateDbContext([]);

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

                // La venta debe estar anulada.
                Assert.Equal(
                    EstadoVenta.Anulada,
                    ventaGuardada.Estado);

                // La venta conserva el vendedor original.
                Assert.Equal(
                    vendedorId,
                    ventaGuardada.VendedorId);

                // Stock inicial: 10
                // Venta: -3
                // Una sola anulación: +3
                Assert.Equal(
                    10m,
                    productoGuardado.StockActual);

                // Debe existir un movimiento de venta
                // y uno de anulación.
                Assert.Equal(2, movimientos.Count);

                var movimientoVenta = Assert.Single(
                    movimientos,
                    m => m.Tipo == TipoMovimientoStock.Venta);

                Assert.Equal(
                    3m,
                    movimientoVenta.Cantidad);

                var movimientoAnulacion = Assert.Single(
                    movimientos,
                    m => m.Tipo == TipoMovimientoStock.AnulacionVenta);

                Assert.Equal(
                    3m,
                    movimientoAnulacion.Cantidad);
            }
            finally
            {
                // Limpieza respetando las relaciones entre tablas.
                await using var limpieza =
                    _factory.CreateDbContext([]);

                // Primero eliminar los movimientos de stock.
                if (productoId > 0)
                {
                    await limpieza.MovimientosStock
                        .Where(m => m.Producto.Id == productoId)
                        .ExecuteDeleteAsync();
                }

                // Eliminar detalles y venta.
                if (ventaId > 0)
                {
                    await limpieza.DetallesVenta
                        .Where(d => d.VentaId == ventaId)
                        .ExecuteDeleteAsync();

                    await limpieza.Ventas
                        .Where(v => v.Id == ventaId)
                        .ExecuteDeleteAsync();
                }

                // Eliminar el producto.
                if (productoId > 0)
                {
                    await limpieza.Productos
                        .Where(p => p.Id == productoId)
                        .ExecuteDeleteAsync();
                }

                // Eliminar la categoría.
                if (categoriaId > 0)
                {
                    await limpieza.Categorias
                        .Where(c => c.Id == categoriaId)
                        .ExecuteDeleteAsync();
                }

                // Eliminar el cliente.
                if (clienteId > 0)
                {
                    await limpieza.Clientes
                        .Where(c => c.Id == clienteId)
                        .ExecuteDeleteAsync();
                }

                // Eliminar el vendedor al final,
                // después de eliminar las ventas asociadas.
                if (vendedorId > 0)
                {
                    await limpieza.Usuarios
                        .Where(u => u.Id == vendedorId)
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
                dni: Random.Shared.Next(
                    10_000_000, 100_000_000).ToString(),
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

            // Crear un vendedor activo para registrar la venta.
            var vendedor = new Usuario(
                nombreUsuario: $"vendedor_{Guid.NewGuid():N}",
                nombreCompleto: "Vendedor de prueba crédito",
                passwordHash: "hash-de-prueba",
                rol: RolUsuario.Vendedor);

            // Guardar los datos iniciales.
            context.Clientes.Add(cliente);
            context.Productos.Add(producto);
            context.Usuarios.Add(vendedor);

            await context.SaveChangesAsync();

            // Crear la cuenta corriente con límite de $125.000.
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
                new CuentaCorrienteRepository(context),
                new UsuarioRepository(context));

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

            // Registrar la venta indicando el vendedor.
            int ventaId = await registrarVenta.EjecutarAsync(
                solicitud,
                vendedor.Id);

            // Comprobar la situación anterior a la anulación.
            context.ChangeTracker.Clear();

            var cuentaAntes = await context.CuentasCorrientes
                .Include(c => c.Movimientos)
                .SingleAsync(c => c.ClienteId == cliente.Id);

            Assert.Equal(
                50000m,
                cuentaAntes.SaldoActual);

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

            await anularVenta.EjecutarAsync(ventaId, vendedor.Id);

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

            // Verificar el estado de la venta.
            Assert.Equal(
                EstadoVenta.Anulada,
                ventaGuardada.Estado);

            // La venta debe conservar su vendedor.
            Assert.Equal(
                vendedor.Id,
                ventaGuardada.VendedorId);

            // El stock debe regresar a su valor inicial.
            Assert.Equal(
                10m,
                productoGuardado.StockActual);

            // La deuda debe quedar en cero.
            Assert.Equal(
                0m,
                cuentaGuardada.SaldoActual);

            // El crédito disponible debe volver a $125.000.
            Assert.Equal(
                125000m,
                cuentaGuardada.LimiteDeuda -
                cuentaGuardada.SaldoActual);

            // El débito original permanece registrado.
            var debito = Assert.Single(
                cuentaGuardada.Movimientos,
                m => m.EsDebito());

            Assert.Equal(
                50000m,
                debito.Importe);

            Assert.Equal(
                ventaId,
                debito.VentaId);

            // La anulación genera un crédito compensatorio.
            var credito = Assert.Single(
                cuentaGuardada.Movimientos,
                m => !m.EsDebito());

            Assert.Equal(
                50000m,
                credito.Importe);

            Assert.Equal(
                ventaId,
                credito.VentaId);

            // Deben existir exactamente dos movimientos:
            // el débito original y el crédito compensatorio.
            Assert.Equal(
                2,
                cuentaGuardada.Movimientos.Count);

            // Sin Commit: los datos se revierten al finalizar.
        }



        [Fact]
        public async Task AnularVentaACredito_ConPagoParcial_NoDebeAnular()
        {
            await using var context = _factory.CreateDbContext([]);

            await using var transaccion =
                await context.Database.BeginTransactionAsync();

            // ARRANGE: crear cliente, categoría y producto.
            var cliente = new ClienteFisico(
                nombre: "Cliente",
                apellido: "PagoParcial",
                dni: Random.Shared.Next(
                    10_000_000, 100_000_000).ToString(),
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

            // Crear un vendedor válido y activo.
            var vendedor = new Usuario(
                nombreUsuario: $"vendedor_{Guid.NewGuid():N}",
                nombreCompleto: "Vendedor pago parcial",
                passwordHash: "hash-de-prueba",
                rol: RolUsuario.Vendedor);

            // Guardar los datos iniciales.
            context.Clientes.Add(cliente);
            context.Productos.Add(producto);
            context.Usuarios.Add(vendedor);

            await context.SaveChangesAsync();

            // Crear la cuenta corriente.
            var cuenta = new CuentaCorriente(
                cliente.Id,
                125000m);

            context.CuentasCorrientes.Add(cuenta);

            await context.SaveChangesAsync();

            // Registrar una venta a crédito por $50.000.
            var registrar = new RegistrarVenta(
                new VentaRepository(context),
                new ClienteRepository(context),
                new ProductoRepository(context),
                new MovimientoStockRepository(context),
                context,
                new CuentaCorrienteRepository(context),
                new UsuarioRepository(context));

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

            // Registrar la venta indicando el vendedor.
            int ventaId = await registrar.EjecutarAsync(
                solicitud,
                vendedor.Id);

            // Simular un pago parcial de $20.000.
            cuenta.RegistrarCredito(
                20000m,
                "Pago parcial del cliente");

            await context.SaveChangesAsync();

            context.ChangeTracker.Clear();

            // ACT: intentar anular la venta.
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
                () => anular.EjecutarAsync(ventaId, vendedor.Id));

            // Consultar los datos realmente persistidos.
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

            // ASSERT: la venta debe continuar confirmada.
            Assert.Equal(
                EstadoVenta.Confirmada,
                ventaGuardada.Estado);

            // La venta debe conservar su vendedor.
            Assert.Equal(
                vendedor.Id,
                ventaGuardada.VendedorId);

            // El stock no debe modificarse.
            Assert.Equal(
                8m,
                productoGuardado.StockActual);

            // La deuda debe reflejar el pago parcial.
            Assert.Equal(
                30000m,
                cuentaGuardada.SaldoActual);

            // Deben existir exactamente dos movimientos:
            // el débito de la venta y el pago parcial.
            Assert.Equal(
                2,
                cuentaGuardada.Movimientos.Count);

            // Verificar el débito original de $50.000.
            var debito = Assert.Single(
                cuentaGuardada.Movimientos,
                m => m.EsDebito());

            Assert.Equal(
                50000m,
                debito.Importe);

            Assert.Equal(
                ventaId,
                debito.VentaId);

            // Verificar el crédito correspondiente al pago.
            var credito = Assert.Single(
                cuentaGuardada.Movimientos,
                m => !m.EsDebito());

            Assert.Equal(
                20000m,
                credito.Importe);

            // Sin Commit: los datos se revierten al finalizar.
        }






    }
}
