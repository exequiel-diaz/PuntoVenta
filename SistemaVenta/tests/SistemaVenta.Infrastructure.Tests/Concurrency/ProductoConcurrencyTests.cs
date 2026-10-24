
using Microsoft.EntityFrameworkCore;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Infrastructure.Tests.Persistence;
using Xunit;

namespace SistemaVenta.Infrastructure.Tests.Concurrency;

public class ProductoConcurrencyTests
{
    private readonly TestDbContextFactory _factory = new();

    [Fact]
    public async Task DosOperacionesSobreElMismoProducto_DebeDetectarConflicto()
    {
        // ARRANGE: datos iniciales.
        int productoId;
        int categoriaId;

        await using (var preparacion = _factory.CreateDbContext([]))
        {
            var categoria = new Categoria(
                $"Concurrencia-{Guid.NewGuid():N}",
                "Categoría de prueba");

            var producto = new Producto(
                codigoInterno: $"CON-{Guid.NewGuid():N}",
                nombre: "Producto concurrencia",
                precioVenta: 10000m,
                costo: 5000m,
                stockMinimo: 1m,
                categoria: categoria);

            producto.AumentarStock(10m);

            preparacion.Productos.Add(producto);
            await preparacion.SaveChangesAsync();

            productoId = producto.Id;
            categoriaId = categoria.Id;
        }

        try
        {
            // Dos contextos independientes consultan
            // el mismo producto.
            await using var contextoA =
                _factory.CreateDbContext([]);

            await using var contextoB =
                _factory.CreateDbContext([]);

            var productoA = await contextoA.Productos
                .SingleAsync(p => p.Id == productoId);

            var productoB = await contextoB.Productos
                .SingleAsync(p => p.Id == productoId);

            Assert.Equal(10m, productoA.StockActual);
            Assert.Equal(10m, productoB.StockActual);

            // Operación A: vender 4 unidades.
            productoA.DisminuirStock(4m);
            await contextoA.SaveChangesAsync();

            // Operación B: intenta vender 3 unidades
            // utilizando información desactualizada.
            productoB.DisminuirStock(3m);

            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
                () => contextoB.SaveChangesAsync());

            // Verificación desde un tercer contexto.
            await using var verificacion =
                _factory.CreateDbContext([]);

            var productoGuardado = await verificacion.Productos
                .AsNoTracking()
                .SingleAsync(p => p.Id == productoId);

            Assert.Equal(6m, productoGuardado.StockActual);
        }
        finally
        {
            // Limpieza de los datos de prueba.
            await using var limpieza =
                _factory.CreateDbContext([]);

            var producto = await limpieza.Productos
                .SingleOrDefaultAsync(p => p.Id == productoId);

            if (producto is not null)
            {
                limpieza.Productos.Remove(producto);
                await limpieza.SaveChangesAsync();
            }

            var categoria = await limpieza.Categorias
                .SingleOrDefaultAsync(c => c.Id == categoriaId);

            if (categoria is not null)
            {
                limpieza.Categorias.Remove(categoria);
                await limpieza.SaveChangesAsync();
            }
        }
    }
}
