
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;
using SistemaVenta.Infrastructure.Persistence;
using SistemaVenta.Infrastructure.Persistence.Repositories;
using SistemaVenta.Integration.Tests.Infrastructure;
using Xunit;

namespace SistemaVenta.Integration.Tests.Informes;

public class InformeVentasRepositoryTests
{
    [Fact]
    public async Task ObtenerVentasDiarias_DebeAgruparPorFechaArgentina()
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


        var diaArgentina =
            await PeriodoPruebaInformes.ObtenerDiaArgentinaLibreAsync(
                contexto);

        // Inicio del día argentino expresado en UTC.
        var fechaDesde = DateTime.SpecifyKind(
            diaArgentina.AddHours(3),
            DateTimeKind.Utc);

        // Inicio del día siguiente, menos un tick.
        var fechaHasta = fechaDesde
            .AddDays(1)
            .AddTicks(-1);

        // Venta realizada a las 22:30 de Argentina.
        var fechaVentaUtc = DateTime.SpecifyKind(
            diaArgentina.AddDays(1).AddHours(1).AddMinutes(30),
            DateTimeKind.Utc);


        // Evitar resultados contaminados por ventas previas.
        var existenVentas = await contexto.Ventas
            .AnyAsync(v =>
                v.Estado == EstadoVenta.Confirmada &&
                v.Fecha >= fechaDesde &&
                v.Fecha <= fechaHasta);

        Assert.False(
            existenVentas,
            "El período elegido contiene ventas confirmadas previas.");

        var identificador = Guid.NewGuid().ToString("N");

        var categoria = new Categoria(
            $"Categoría informe {identificador}");

        var producto = new Producto(
            $"INF-{identificador}",
            "Mouse de prueba",
            20000m,
            12000m,
            2m,
            categoria);

        producto.AumentarStock(10m);

        var venta = new Venta(null);

        venta.AgregarDetalle(
            producto,
            2m,
            20000m);

        venta.Confirmar();

        contexto.Ventas.Add(venta);

        await contexto.SaveChangesAsync();


        contexto.Entry(venta)
            .Property(v => v.Fecha)
            .CurrentValue = fechaVentaUtc;

        await contexto.SaveChangesAsync();

        contexto.ChangeTracker.Clear();

        var repositorio =
            new InformeVentasRepository(contexto);

        // Act
        var resultado =
            await repositorio.ObtenerVentasDiariasAsync(
                fechaDesde,
                fechaHasta);

        // Assert
        var dia = Assert.Single(resultado);

        Assert.Equal(diaArgentina, dia.Fecha);

        Assert.Equal(1, dia.CantidadVentas);

        Assert.Equal(40000m, dia.TotalVendido);

        // No hacemos Commit.
        // PostgreSQL revierte los datos al finalizar.
    }


    [Fact]
    public async Task ObtenerVentasDiarias_ConMismaFechaUtc_SeparaDiasArgentinos()
    {
        // Arrange
        await using var aplicacion =
            new SistemaVentaWebApplicationFactory();

        using var scope = aplicacion.Services.CreateScope();

        var contexto = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        await using var transaccion =
            await contexto.Database.BeginTransactionAsync();

        // Obtenemos un día libre de ventas.
        var diaArgentina =
            await PeriodoPruebaInformes.ObtenerDiaArgentinaLibreAsync(
                contexto);

        // Inicio del primer día argentino en UTC.
        var fechaDesde = DateTime.SpecifyKind(
            diaArgentina.AddHours(3),
            DateTimeKind.Utc);

        // Incluimos dos días argentinos completos.
        var fechaHasta = fechaDesde
            .AddDays(2)
            .AddTicks(-1);

        // Verificamos que no haya ventas confirmadas previas.
        var existenVentas = await contexto.Ventas
            .AnyAsync(v =>
                v.Estado == EstadoVenta.Confirmada &&
                v.Fecha >= fechaDesde &&
                v.Fecha <= fechaHasta);

        Assert.False(
            existenVentas,
            "El período elegido contiene ventas confirmadas previas.");

        var identificador = Guid.NewGuid().ToString("N");

        var categoria = new Categoria(
            $"Categoría informe {identificador}");

        var producto = new Producto(
            $"INF-{identificador}",
            "Mouse de prueba",
            20000m,
            12000m,
            2m,
            categoria);

        producto.AumentarStock(10m);

        // Creamos la primera venta: $40.000.
        var venta1 = new Venta(null);
        venta1.AgregarDetalle(producto, 2m, 20000m);
        venta1.Confirmar();

        // Creamos la segunda venta: $25.000.
        var venta2 = new Venta(null);
        venta2.AgregarDetalle(producto, 1m, 25000m);
        venta2.Confirmar();

        // Guardamos ambas ventas.
        contexto.Ventas.AddRange(venta1, venta2);
        await contexto.SaveChangesAsync();

        // Primera venta:
        // 22:30 del primer día argentino.
        // 01:30 UTC del día siguiente.
        contexto.Entry(venta1)
            .Property(v => v.Fecha)
            .CurrentValue = DateTime.SpecifyKind(
                diaArgentina.AddDays(1)
                    .AddHours(1)
                    .AddMinutes(30),
                DateTimeKind.Utc);

        // Segunda venta:
        // 10:00 del segundo día argentino.
        // 13:00 UTC del mismo día que la primera venta.
        contexto.Entry(venta2)
            .Property(v => v.Fecha)
            .CurrentValue = DateTime.SpecifyKind(
                diaArgentina.AddDays(1).AddHours(13),
                DateTimeKind.Utc);

        // Guardamos las fechas actualizadas.
        await contexto.SaveChangesAsync();

        contexto.ChangeTracker.Clear();

        var repositorio =
            new InformeVentasRepository(contexto);

        // Act
        var resultado =
            await repositorio.ObtenerVentasDiariasAsync(
                fechaDesde,
                fechaHasta);

        // Assert
        Assert.Equal(2, resultado.Count);

        // Primer día argentino.
        Assert.Equal(
            diaArgentina,
            resultado[0].Fecha);

        Assert.Equal(1, resultado[0].CantidadVentas);
        Assert.Equal(40000m, resultado[0].TotalVendido);

        // Segundo día argentino.
        Assert.Equal(
            diaArgentina.AddDays(1),
            resultado[1].Fecha);

        Assert.Equal(1, resultado[1].CantidadVentas);
        Assert.Equal(25000m, resultado[1].TotalVendido);

        // Total de ambos días.
        Assert.Equal(
            65000m,
            resultado.Sum(v => v.TotalVendido));

        // Sin Commit: la transacción se revierte.
    }


    [Fact]
    public async Task Informes_ResumenYVentasDiarias_DebenCoincidir()
    {
        // Arrange
        await using var aplicacion =
            new SistemaVentaWebApplicationFactory();

        using var scope = aplicacion.Services.CreateScope();

        var contexto = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        await using var transaccion =
            await contexto.Database.BeginTransactionAsync();


        var diaArgentina =
            await PeriodoPruebaInformes.ObtenerDiaArgentinaLibreAsync(
                contexto);

        var fechaDesde = DateTime.SpecifyKind(
            diaArgentina.AddHours(3),
            DateTimeKind.Utc);

        // Incluimos dos días argentinos completos.
        var fechaHasta = fechaDesde
            .AddDays(2)
            .AddTicks(-1);


        // Comprobamos que no existan ventas anteriores
        // que puedan alterar los resultados.
        var existenVentas = await contexto.Ventas
            .AnyAsync(v =>
                v.Estado == EstadoVenta.Confirmada &&
                v.Fecha >= fechaDesde &&
                v.Fecha <= fechaHasta);

        Assert.False(
            existenVentas,
            "El período elegido contiene ventas confirmadas previas.");

        var identificador = Guid.NewGuid().ToString("N");

        var categoria = new Categoria(
            $"Categoría informe {identificador}");

        var producto = new Producto(
            $"INF-{identificador}",
            "Mouse de prueba",
            20000m,
            12000m,
            2m,
            categoria);

        producto.AumentarStock(10m);

        // Venta del 15 de enero: $40.000.
        var venta1 = new Venta(null);
        venta1.AgregarDetalle(producto, 2m, 20000m);
        venta1.Confirmar();

        // Venta del 16 de enero: $25.000.
        var venta2 = new Venta(null);
        venta2.AgregarDetalle(producto, 1m, 25000m);
        venta2.Confirmar();

        contexto.Ventas.AddRange(venta1, venta2);
        await contexto.SaveChangesAsync();


        // Primera venta: 22:30 del primer día argentino.
        contexto.Entry(venta1)
            .Property(v => v.Fecha)
            .CurrentValue = DateTime.SpecifyKind(
                diaArgentina.AddDays(1)
                    .AddHours(1)
                    .AddMinutes(30),
                DateTimeKind.Utc);

        // Segunda venta: 10:00 del segundo día argentino.
        contexto.Entry(venta2)
            .Property(v => v.Fecha)
            .CurrentValue = DateTime.SpecifyKind(
                diaArgentina.AddDays(1).AddHours(13),
                DateTimeKind.Utc);


        await contexto.SaveChangesAsync();

        contexto.ChangeTracker.Clear();

        var repositorio =
            new InformeVentasRepository(contexto);

        // Act
        var resumen =
            await repositorio.ObtenerVentasPorPeriodoAsync(
                fechaDesde,
                fechaHasta);

        var ventasDiarias =
            await repositorio.ObtenerVentasDiariasAsync(
                fechaDesde,
                fechaHasta);

        // Assert: verificamos los valores esperados.
        Assert.Equal(2, resumen.CantidadVentas);
        Assert.Equal(65000m, resumen.TotalVendido);
        Assert.Equal(32500m, resumen.TicketPromedio);

        Assert.Equal(2, ventasDiarias.Count);

        // Verificamos que ambos informes coincidan.
        Assert.Equal(
            resumen.CantidadVentas,
            ventasDiarias.Sum(v => v.CantidadVentas));

        Assert.Equal(
            resumen.TotalVendido,
            ventasDiarias.Sum(v => v.TotalVendido));

        // Sin Commit: PostgreSQL revierte los datos.
    }


    [Fact]
    public async Task Informes_DebenExcluirVentasPendientesYAnuladas()
    {
        // Arrange
        await using var aplicacion =
            new SistemaVentaWebApplicationFactory();

        using var scope = aplicacion.Services.CreateScope();

        var contexto = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        await using var transaccion =
            await contexto.Database.BeginTransactionAsync();

        var fechaDesde = new DateTime(
            2020, 1, 15, 3, 0, 0, DateTimeKind.Utc);

        var fechaHasta = new DateTime(
            2020, 1, 16, 3, 0, 0, DateTimeKind.Utc)
            .AddTicks(-1);

        var existenVentas = await contexto.Ventas
            .AnyAsync(v =>
                v.Estado == EstadoVenta.Confirmada &&
                v.Fecha >= fechaDesde &&
                v.Fecha <= fechaHasta);

        Assert.False(
            existenVentas,
            "El período contiene ventas confirmadas previas.");

        var identificador = Guid.NewGuid().ToString("N");

        var categoria = new Categoria(
            $"Categoría informe {identificador}");

        var producto = new Producto(
            $"INF-{identificador}",
            "Mouse de prueba",
            20000m,
            12000m,
            2m,
            categoria);

        producto.AumentarStock(10m);

        // Venta confirmada: $40.000
        var confirmada = new Venta(null);
        confirmada.AgregarDetalle(producto, 2m, 20000m);
        confirmada.Confirmar();

        // Venta pendiente: $20.000
        var pendiente = new Venta(null);
        pendiente.AgregarDetalle(producto, 1m, 20000m);

        // Venta anulada: $30.000
        var anulada = new Venta(null);
        anulada.AgregarDetalle(producto, 1m, 30000m);
        anulada.Confirmar();
        anulada.Anular();

        contexto.Ventas.AddRange(
            confirmada,
            pendiente,
            anulada);

        await contexto.SaveChangesAsync();

        var fechaVentaUtc = new DateTime(
            2020, 1, 16, 1, 30, 0, DateTimeKind.Utc);

        foreach (var venta in new[]
        {
        confirmada,
        pendiente,
        anulada
    })
        {
            contexto.Entry(venta)
                .Property(v => v.Fecha)
                .CurrentValue = fechaVentaUtc;
        }

        await contexto.SaveChangesAsync();

        contexto.ChangeTracker.Clear();

        var repositorio =
            new InformeVentasRepository(contexto);

        // Act
        var resumen =
            await repositorio.ObtenerVentasPorPeriodoAsync(
                fechaDesde,
                fechaHasta);

        var ventasDiarias =
            await repositorio.ObtenerVentasDiariasAsync(
                fechaDesde,
                fechaHasta);

        // Assert
        Assert.Equal(1, resumen.CantidadVentas);
        Assert.Equal(40000m, resumen.TotalVendido);
        Assert.Equal(40000m, resumen.TicketPromedio);

        var dia = Assert.Single(ventasDiarias);

        Assert.Equal(
            new DateTime(2020, 1, 15),
            dia.Fecha);

        Assert.Equal(1, dia.CantidadVentas);
        Assert.Equal(40000m, dia.TotalVendido);

        // Sin Commit: se revierten todos los registros.
    }


    [Fact]
    public async Task Informes_DebenRespetarLimitesDelPeriodo()
    {
        // Arrange
        await using var aplicacion =
            new SistemaVentaWebApplicationFactory();

        using var scope = aplicacion.Services.CreateScope();

        var contexto = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        await using var transaccion =
            await contexto.Database.BeginTransactionAsync();

        var fechaDesde = new DateTime(
            2020, 1, 15, 3, 0, 0, DateTimeKind.Utc);

        var fechaHasta = new DateTime(
            2020, 1, 16, 3, 0, 0, DateTimeKind.Utc)
            .AddTicks(-1);

        // Verificamos que no existan ventas confirmadas
        // previas dentro del período consultado.
        var existenVentas = await contexto.Ventas
            .AnyAsync(v =>
                v.Estado == EstadoVenta.Confirmada &&
                v.Fecha >= fechaDesde &&
                v.Fecha <= fechaHasta);

        Assert.False(
            existenVentas,
            "El período contiene ventas confirmadas previas.");

        var identificador = Guid.NewGuid().ToString("N");

        var categoria = new Categoria(
            $"Categoría informe {identificador}");

        var producto = new Producto(
            $"INF-{identificador}",
            "Mouse de prueba",
            20000m,
            12000m,
            2m,
            categoria);

        producto.AumentarStock(10m);

        var ventaAnterior = new Venta(null);
        ventaAnterior.AgregarDetalle(producto, 1m, 10000m);
        ventaAnterior.Confirmar();

        var ventaDentro = new Venta(null);
        ventaDentro.AgregarDetalle(producto, 1m, 20000m);
        ventaDentro.Confirmar();

        var ventaPosterior = new Venta(null);
        ventaPosterior.AgregarDetalle(producto, 1m, 30000m);
        ventaPosterior.Confirmar();

        contexto.Ventas.AddRange(
            ventaAnterior,
            ventaDentro,
            ventaPosterior);

        await contexto.SaveChangesAsync();

        // 14/01/2020 23:59:59 en Argentina.
        contexto.Entry(ventaAnterior)
            .Property(v => v.Fecha)
            .CurrentValue = new DateTime(
                2020, 1, 15, 2, 59, 59, DateTimeKind.Utc);

        // 15/01/2020 00:00:00 en Argentina.
        contexto.Entry(ventaDentro)
            .Property(v => v.Fecha)
            .CurrentValue = new DateTime(
                2020, 1, 15, 3, 0, 0, DateTimeKind.Utc);

        // 16/01/2020 00:00:00 en Argentina.
        contexto.Entry(ventaPosterior)
            .Property(v => v.Fecha)
            .CurrentValue = new DateTime(
                2020, 1, 16, 3, 0, 0, DateTimeKind.Utc);

        await contexto.SaveChangesAsync();

        contexto.ChangeTracker.Clear();

        var repositorio =
            new InformeVentasRepository(contexto);

        // Act
        var resumen =
            await repositorio.ObtenerVentasPorPeriodoAsync(
                fechaDesde,
                fechaHasta);

        var ventasDiarias =
            await repositorio.ObtenerVentasDiariasAsync(
                fechaDesde,
                fechaHasta);

        // Assert
        Assert.Equal(1, resumen.CantidadVentas);
        Assert.Equal(20000m, resumen.TotalVendido);
        Assert.Equal(20000m, resumen.TicketPromedio);

        var dia = Assert.Single(ventasDiarias);

        Assert.Equal(
            new DateTime(2020, 1, 15),
            dia.Fecha);

        Assert.Equal(1, dia.CantidadVentas);
        Assert.Equal(20000m, dia.TotalVendido);

        // Sin Commit: los datos se revierten.
    }



}
