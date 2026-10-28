
using Microsoft.EntityFrameworkCore;
using SistemaVenta.Application.UseCases.Facturas;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;
using SistemaVenta.Infrastructure.Persistence.Repositories;
using SistemaVenta.Infrastructure.Repositories;
using SistemaVenta.Infrastructure.Tests.Persistence;
using Xunit;
using SistemaVenta.Infrastructure.Persistence;

namespace SistemaVenta.Infrastructure.Tests.Facturas
{
    public class EmitirFacturaIntegrationTests
    {
        private readonly TestDbContextFactory _factory = new();

        [Fact]
        public async Task EmitirFactura_VentaConfirmada_GuardaFacturaEnPostgreSQL()
        {
            // ARRANGE
            await using var context = _factory.CreateDbContext([]);

            // La transacción permite revertir los datos al finalizar.
            await using var transaccion =
                await context.Database.BeginTransactionAsync();

            var cliente = new ClienteFisico(
                nombre: "Cliente",
                apellido: "Facturacion",
                dni: Random.Shared.Next(
                    10_000_000, 100_000_000).ToString(),
                telefono: "3811234567",
                email: $"factura-{Guid.NewGuid():N}@test.com",
                direccion: "Tucumán");

            var categoria = new Categoria(
                $"Componentes-{Guid.NewGuid():N}");

            var producto = new Producto(
                codigoInterno: $"FAC-{Guid.NewGuid():N}",
                nombre: "Memoria RAM 8 GB",
                precioVenta: 15000m,
                costo: 10000m,
                stockMinimo: 2m,
                categoria: categoria);

            context.Clientes.Add(cliente);
            context.Productos.Add(producto);

            await context.SaveChangesAsync();

            var venta = new Venta(cliente.Id);

            venta.AgregarDetalle(
                producto,
                cantidad: 2m,
                precioUnitario: 15000m);

            venta.Confirmar();

            context.Ventas.Add(venta);

            await context.SaveChangesAsync();

            // Ejecutamos el caso de uso real.

            var emitirFactura = new EmitirFactura(
                new VentaRepository(context),
                new DocumentoComercialRepository(context),
                context,
                new TransaccionVentaPostgres(context));


            // ACT
            var facturaId = await emitirFactura.EjecutarAsync(
                venta.Id,
                TipoFactura.B);

            // Eliminamos las entidades rastreadas para volver
            // a consultar los valores persistidos.
            context.ChangeTracker.Clear();

            var facturaGuardada = await context.DocumentosComerciales
                .OfType<Factura>()
                .AsNoTracking()
                .SingleAsync(f => f.Id == facturaId);

            // ASSERT
            Assert.Equal(venta.Id, facturaGuardada.VentaId);

            Assert.Equal(TipoFactura.B, facturaGuardada.Tipo);

            Assert.Equal(30000m, facturaGuardada.ImporteTotal);

            Assert.StartsWith(
                "INT-",
                facturaGuardada.Numero);

            Assert.Equal(
                EstadoDocumento.Emitido,
                facturaGuardada.Estado);

            // Sin Commit: los datos se revierten al
            // finalizar la prueba.
        }


        [Fact]
        public async Task EmitirFactura_VentaYaFacturada_NoPermiteSegundaFactura()
        {
            // ARRANGE
            await using var context = _factory.CreateDbContext([]);

            await using var transaccion =
                await context.Database.BeginTransactionAsync();

            var cliente = new ClienteFisico(
                nombre: "Cliente",
                apellido: "FacturaDuplicada",
                dni: Random.Shared.Next(
                    10_000_000, 100_000_000).ToString(),
                telefono: "3811234567",
                email: $"duplicada-{Guid.NewGuid():N}@test.com",
                direccion: "Tucumán");

            var categoria = new Categoria(
                $"Componentes-{Guid.NewGuid():N}");

            var producto = new Producto(
                codigoInterno: $"DUP-{Guid.NewGuid():N}",
                nombre: "Disco SSD",
                precioVenta: 25000m,
                costo: 15000m,
                stockMinimo: 2m,
                categoria: categoria);

            context.Clientes.Add(cliente);
            context.Productos.Add(producto);

            await context.SaveChangesAsync();

            var venta = new Venta(cliente.Id);

            venta.AgregarDetalle(
                producto,
                cantidad: 2m,
                precioUnitario: 25000m);

            venta.Confirmar();

            context.Ventas.Add(venta);

            await context.SaveChangesAsync();


            var emitirFactura = new EmitirFactura(
                new VentaRepository(context),
                new DocumentoComercialRepository(context),
                context,
                new TransaccionVentaPostgres(context));


            // ACT: primera emisión.
            await emitirFactura.EjecutarAsync(
                venta.Id,
                TipoFactura.B);

            // ACT: intentar emitir una segunda factura.
            var excepcion =
                await Assert.ThrowsAsync<InvalidOperationException>(
                    () => emitirFactura.EjecutarAsync(
                        venta.Id,
                        TipoFactura.B));

            // ASSERT
            Assert.Equal(
                "La venta ya tiene una factura registrada.",
                excepcion.Message);

            context.ChangeTracker.Clear();

            var cantidadFacturas =
                await context.DocumentosComerciales
                    .OfType<Factura>()
                    .CountAsync(f => f.VentaId == venta.Id);

            Assert.Equal(1, cantidadFacturas);
        }


    }
}
