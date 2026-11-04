
using Microsoft.EntityFrameworkCore;
using SistemaVenta.Application.DTOs.Informes;
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Infrastructure.Persistence.Repositories
{
    public class InformeVentasRepository : IInformeVentasRepository
    {
        private readonly AppDbContext _context;

        public InformeVentasRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<InformeVentasPorPeriodoDto> ObtenerVentasPorPeriodoAsync(
            DateTime fechaDesde,
            DateTime fechaHasta)
        {
            var estadisticas = await _context.Ventas
                .AsNoTracking()
                .Where(v =>
                    v.Estado == EstadoVenta.Confirmada &&
                    v.Fecha >= fechaDesde &&
                    v.Fecha <= fechaHasta)
                .Select(v => new
                {
                    Total = v.Detalles.Sum(d =>
                        d.Cantidad * d.PrecioUnitario)
                })
                .GroupBy(v => 1)
                .Select(g => new
                {
                    CantidadVentas = g.Count(),
                    TotalVendido = g.Sum(v => v.Total)
                })
                .FirstOrDefaultAsync();

            var cantidad = estadisticas?.CantidadVentas ?? 0;
            var total = estadisticas?.TotalVendido ?? 0m;

            return new InformeVentasPorPeriodoDto
            {
                FechaDesde = fechaDesde,
                FechaHasta = fechaHasta,
                CantidadVentas = cantidad,
                TotalVendido = total,
                TicketPromedio = cantidad > 0
                    ? Math.Round(total / cantidad, 2)
                    : 0m
            };
        }


        public async Task<List<ProductoMasVendidoDto>> ObtenerProductosMasVendidosAsync(
            DateTime fechaDesde,
            DateTime fechaHasta,
            int cantidad)
        {
            var productos = await _context.Ventas
                .AsNoTracking()
                .Where(v =>
                    v.Estado == EstadoVenta.Confirmada &&
                    v.Fecha >= fechaDesde &&
                    v.Fecha <= fechaHasta)
                .SelectMany(v => v.Detalles)
                .GroupBy(d => new
                {
                    d.ProductoId,
                    d.Producto.Nombre
                })
                .Select(g => new ProductoMasVendidoDto
                {
                    ProductoId = g.Key.ProductoId,
                    NombreProducto = g.Key.Nombre,
                    UnidadesVendidas = g.Sum(d => d.Cantidad),
                    TotalVendido = g.Sum(d =>
                        d.Cantidad * d.PrecioUnitario)
                })
                .OrderByDescending(p => p.UnidadesVendidas)
                .ThenBy(p => p.NombreProducto)
                .Take(cantidad)
                .ToListAsync();

            return productos;
        }



        public async Task<List<VentaDiariaDto>> ObtenerVentasDiariasAsync(
            DateTime fechaDesde,
            DateTime fechaHasta)
        {
            var zonaArgentina = TimeZoneInfo.FindSystemTimeZoneById(
                "America/Argentina/Buenos_Aires");

            var ventas = await _context.Ventas
                .AsNoTracking()
                .Where(v =>
                    v.Estado == EstadoVenta.Confirmada &&
                    v.Fecha >= fechaDesde &&
                    v.Fecha <= fechaHasta)
                .Select(v => new
                {
                    v.Fecha,
                    Total = v.Detalles.Sum(d =>
                        d.Cantidad * d.PrecioUnitario)
                })
                .ToListAsync();

            var resultado = ventas
                .GroupBy(v =>
                    TimeZoneInfo.ConvertTimeFromUtc(
                        DateTime.SpecifyKind(v.Fecha, DateTimeKind.Utc),
                        zonaArgentina).Date)
                .Select(g => new VentaDiariaDto
                {
                    Fecha = g.Key,
                    CantidadVentas = g.Count(),
                    TotalVendido = g.Sum(v => v.Total)
                })
                .OrderBy(v => v.Fecha)
                .ToList();

            return resultado;
        }


    }
}
