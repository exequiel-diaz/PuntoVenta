
using SistemaVenta.Application.DTOs.Dashboard;
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Application.UseCases.Dashboard;

public class ObtenerResumenDashboard
{
    private readonly IVentaRepository _ventas;
    private readonly IProductoRepository _productos;

    public ObtenerResumenDashboard(
        IVentaRepository ventas,
        IProductoRepository productos)
    {
        _ventas = ventas;
        _productos = productos;
    }

    public async Task<ResumenDashboardDto> EjecutarAsync(
        DateTime fechaLocal,
        TimeZoneInfo zonaHoraria)
    {
        var hoy = fechaLocal.Date;
        var inicio = hoy.AddDays(-6);

        var desdeUtc = TimeZoneInfo.ConvertTimeToUtc(
            inicio,
            zonaHoraria);

        var hastaUtc = TimeZoneInfo.ConvertTimeToUtc(
            hoy.AddDays(1),
            zonaHoraria);

        var ventas = await _ventas.ObtenerPorPeriodoAsync(
            desdeUtc,
            hastaUtc);

        var productos = await _productos.ObtenerTodosAsync();

        var ventasConfirmadas = ventas
            .Where(v => v.Estado == EstadoVenta.Confirmada)
            .ToList();

        var resumen = new ResumenDashboardDto();

        var ventasHoy = ventasConfirmadas
            .Where(v =>
                TimeZoneInfo.ConvertTimeFromUtc(
                    DateTime.SpecifyKind(v.Fecha, DateTimeKind.Utc),
                    zonaHoraria).Date == hoy)
            .ToList();

        resumen.VentasHoy = ventasHoy.Sum(v => v.Total);

        resumen.CantidadVentasHoy = ventasHoy.Count;

        var productosActivos = productos
            .Where(p => p.Estado == EstadoProducto.Activo)
            .ToList();

        resumen.ProductosActivos = productosActivos.Count;

        resumen.ProductosSinStock = productosActivos.Count(
            p => p.StockActual <= 0);

        resumen.ProductosStockBajo = productosActivos.Count(
            p => p.StockActual > 0 &&
                 p.StockActual <= p.StockMinimo);

        for (int i = 0; i < 7; i++)
        {
            var fecha = inicio.AddDays(i);

            var total = ventasConfirmadas
                .Where(v =>
                    TimeZoneInfo.ConvertTimeFromUtc(
                        DateTime.SpecifyKind(
                            v.Fecha, DateTimeKind.Utc),
                        zonaHoraria).Date == fecha)
                .Sum(v => v.Total);

            resumen.VentasUltimosDias.Add(
                new VentasPorDiaDto
                {
                    Dia = fecha.ToString("dd/MM"),
                    Total = total
                });
        }


        var inicioRankingLocal = hoy.AddDays(-29);

        var inicioRankingUtc = TimeZoneInfo.ConvertTimeToUtc(
            inicioRankingLocal,
            zonaHoraria);

        var finRankingUtc = TimeZoneInfo.ConvertTimeToUtc(
            hoy.AddDays(1),
            zonaHoraria);

        var productosMasVendidos =
            await _ventas.ObtenerProductosMasVendidosAsync(
                inicioRankingUtc,
                finRankingUtc,
                5);

        resumen.ProductosMasVendidos =
            productosMasVendidos.ToList();


        return resumen;
    }
}
