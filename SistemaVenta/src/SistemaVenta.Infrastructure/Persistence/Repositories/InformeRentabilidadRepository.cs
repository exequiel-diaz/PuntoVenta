
using Microsoft.EntityFrameworkCore;
using SistemaVenta.Application.DTOs.Informes;
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Infrastructure.Persistence.Repositories;

public class InformeRentabilidadRepository
    : IInformeRentabilidadRepository
{
    private readonly AppDbContext _context;

    public InformeRentabilidadRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<InformeRentabilidadDto> ObtenerAsync(
        DateTime fechaDesde,
        DateTime fechaHasta,
        int top)
    {

        var ventas = _context.Ventas
            .AsNoTracking()
            .Where(v =>
                v.Estado == EstadoVenta.Confirmada &&
                v.Fecha >= fechaDesde &&
                v.Fecha <= fechaHasta &&
                v.Detalles.Any());


        // Una venta es analizable solamente cuando
        // todos sus detalles tienen costo histórico.
        var resumen = await ventas
            .Select(v => new
            {
                TieneCosto = v.Detalles.All(d =>
                    d.CostoUnitario.HasValue),

                Facturacion = v.Detalles.Sum(d =>
                    d.Cantidad * d.PrecioUnitario),

                Costo = v.Detalles.Sum(d =>
                    d.Cantidad * (d.CostoUnitario ?? 0m))
            })
            .ToListAsync();

        var analizables = resumen
            .Where(v => v.TieneCosto)
            .ToList();

        var facturacionAnalizada =
            analizables.Sum(v => v.Facturacion);

        var costoMercaderia =
            analizables.Sum(v => v.Costo);

        // Utilizar exactamente el mismo conjunto de ventas
        // para el ranking por producto.
        var productos = await ventas
            .Where(v => v.Detalles.All(d =>
                d.CostoUnitario.HasValue))
            .SelectMany(v => v.Detalles)
            .GroupBy(d => new
            {
                d.ProductoId,
                d.Producto.Nombre
            })
            .Select(g => new
            {
                ProductoId = g.Key.ProductoId,
                Nombre = g.Key.Nombre,
                Unidades = g.Sum(d => d.Cantidad),
                Facturacion = g.Sum(d =>
                    d.Cantidad * d.PrecioUnitario),
                Costo = g.Sum(d =>
                    d.Cantidad * (d.CostoUnitario ?? 0m))
            })
            .OrderByDescending(p =>
                p.Facturacion - p.Costo)
            .ThenBy(p => p.Nombre)
            .Take(top)
            .ToListAsync();


        // Obtener únicamente las ventas con costo histórico completo.
        // Proyectamos importes por venta para no cargar entidades enteras.
        var ventasDiarias = await ventas
            .Where(v => v.Detalles.Any())
            .Where(v => v.Detalles.All(d =>
                d.CostoUnitario.HasValue))
            .Select(v => new
            {
                v.Fecha,

                Facturacion = v.Detalles.Sum(d =>
                    d.Cantidad * d.PrecioUnitario),

                Costo = v.Detalles.Sum(d =>
                    d.Cantidad * d.CostoUnitario!.Value)
            })
            .ToListAsync();

        // Convertir fechas UTC a fechas de Argentina antes de agrupar.
        var zonaArgentina = TimeZoneInfo.FindSystemTimeZoneById(
            "America/Argentina/Buenos_Aires");

        var agrupadas = ventasDiarias
            .GroupBy(v =>
                TimeZoneInfo.ConvertTimeFromUtc(
                    DateTime.SpecifyKind(v.Fecha, DateTimeKind.Utc),
                    zonaArgentina).Date)
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    Facturacion = g.Sum(x => x.Facturacion),
                    Costo = g.Sum(x => x.Costo)
                });

        // Completar también los días sin ventas.
        var primerDia = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.SpecifyKind(fechaDesde, DateTimeKind.Utc),
            zonaArgentina).Date;

        var ultimoDia = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.SpecifyKind(fechaHasta, DateTimeKind.Utc),
            zonaArgentina).Date;

        var evolucionDiaria = new List<RentabilidadDiariaDto>();

        for (var dia = primerDia; dia <= ultimoDia; dia = dia.AddDays(1))
        {
            agrupadas.TryGetValue(dia, out var valores);

            evolucionDiaria.Add(new RentabilidadDiariaDto
            {
                Fecha = dia,
                Facturacion = valores?.Facturacion ?? 0m,
                Costo = valores?.Costo ?? 0m
            });
        }


        return new InformeRentabilidadDto
        {
            CantidadVentas = resumen.Count,
            VentasConCosto = analizables.Count,
            VentasSinCosto = resumen.Count - analizables.Count,

            FacturacionTotal =
                resumen.Sum(v => v.Facturacion),

            FacturacionAnalizada = facturacionAnalizada,
            CostoMercaderia = costoMercaderia,
            GananciaBruta =
                facturacionAnalizada - costoMercaderia,

            Productos = productos.Select(p =>
                new ProductoRentabilidadDto
                {
                    ProductoId = p.ProductoId,
                    NombreProducto = p.Nombre,
                    UnidadesVendidas = p.Unidades,
                    Facturacion = p.Facturacion,
                    CostoTotal = p.Costo,
                    GananciaBruta =
                        p.Facturacion - p.Costo
                }).ToList(),

            EvolucionDiaria = evolucionDiaria,
        };


    }
}
