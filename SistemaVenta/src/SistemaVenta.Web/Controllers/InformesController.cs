
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaVenta.Application.UseCases.Informes;
using SistemaVenta.Domain.Enums;
using SistemaVenta.Web.Models.Informes;
using SistemaVenta.Application.Interfaces;



namespace SistemaVenta.Web.Controllers;

[Authorize(Roles = nameof(RolUsuario.Administrador))]
public class InformesController : Controller
{
    private readonly ObtenerInformeVentasPorPeriodo _obtenerInformeVentas;
    private readonly ObtenerProductosMasVendidos _obtenerProductosMasVendidos;
    private readonly ObtenerVentasDiarias _obtenerVentasDiarias;
    private readonly ObtenerVentasPorVendedor _obtenerVentasPorVendedor;

    private readonly ObtenerInformeInventario _obtenerInventario;
    private readonly ObtenerInformeMovimientosStock _obtenerMovimientos;
    private readonly IGeneradorInformeMovimientosPdf _generadorMovimientosPdf;

    public InformesController(
        ObtenerInformeVentasPorPeriodo obtenerInformeVentas, 
        ObtenerProductosMasVendidos obtenerProductosMasVendidos,
        ObtenerVentasDiarias obtenerVentasDiarias,
        ObtenerVentasPorVendedor obtenerVentasPorVendedor,
        ObtenerInformeInventario obtenerInventario,
        ObtenerInformeMovimientosStock obtenerMovimientos,
        IGeneradorInformeMovimientosPdf generadorMovimientosPdf)
    {
        _obtenerInformeVentas = obtenerInformeVentas;
        _obtenerProductosMasVendidos = obtenerProductosMasVendidos;
        _obtenerVentasDiarias = obtenerVentasDiarias;
        _obtenerVentasPorVendedor = obtenerVentasPorVendedor;
        _obtenerInventario = obtenerInventario;
        _obtenerMovimientos = obtenerMovimientos;
        _generadorMovimientosPdf = generadorMovimientosPdf;
    }


    [HttpGet]
    public async Task<IActionResult> Index(
        DateTime? fechaDesde,
        DateTime? fechaHasta)
    {
        var hoy = DateTime.UtcNow.Date;

        var desde = fechaDesde?.Date ?? hoy.AddDays(-30);
        var hasta = fechaHasta?.Date ?? hoy;

        if (desde > hasta)
        {
            ModelState.AddModelError(
                "",
                "La fecha inicial no puede ser posterior a la fecha final.");

            return BadRequest(
                "La fecha inicial no puede ser posterior a la fecha final.");
        }

        // Interpretamos las fechas elegidas como días de Argentina.
        var zonaArgentina = TimeZoneInfo.FindSystemTimeZoneById(
            "America/Argentina/Buenos_Aires");

        var inicioLocal = DateTime.SpecifyKind(
            desde.Date,
            DateTimeKind.Unspecified);

        var finLocalExclusivo = DateTime.SpecifyKind(
            hasta.Date.AddDays(1),
            DateTimeKind.Unspecified);

        var inicioUtc = TimeZoneInfo.ConvertTimeToUtc(
            inicioLocal,
            zonaArgentina);

        var finUtcExclusivo = TimeZoneInfo.ConvertTimeToUtc(
            finLocalExclusivo,
            zonaArgentina);

        var finUtcInclusivo = finUtcExclusivo.AddTicks(-1);

        var resumenVentas = await _obtenerInformeVentas
            .EjecutarAsync(inicioUtc, finUtcInclusivo);

        var ventasDiarias = await _obtenerVentasDiarias
            .EjecutarAsync(inicioUtc, finUtcInclusivo);

        var ventasPorVendedor = await _obtenerVentasPorVendedor
            .EjecutarAsync(inicioUtc, finUtcInclusivo);


        var modelo = new DashboardInformesViewModel
        {
            FechaDesde = desde,
            FechaHasta = hasta,
            ResumenVentas = resumenVentas,
            VentasDiarias = ventasDiarias,
            VentasPorVendedor = ventasPorVendedor
        };


        return View(modelo);
    }



    [HttpGet]
    public async Task<IActionResult> VentasPorPeriodo(
        DateTime? fechaDesde,
        DateTime? fechaHasta)
    {

        var hoy = DateTime.UtcNow.Date;

        var desde = fechaDesde ?? hoy.AddDays(-30);
        var hasta = fechaHasta ?? hoy;

        desde = DateTime.SpecifyKind(desde.Date, DateTimeKind.Utc);
        hasta = DateTime.SpecifyKind(hasta.Date, DateTimeKind.Utc);


        if (desde.Date > hasta.Date)
        {
            return BadRequest(
                "La fecha inicial no puede ser posterior a la fecha final.");
        }

        var informe = await _obtenerInformeVentas.EjecutarAsync(
            desde.Date,
            hasta.Date.AddDays(1).AddTicks(-1));

        return View(informe);
    }


    [HttpGet]
    public async Task<IActionResult> ProductosMasVendidos(
        DateTime? fechaDesde,
        DateTime? fechaHasta,
        int cantidad = 10)
    {
        var hoy = DateTime.UtcNow.Date;

        var desde = fechaDesde ?? hoy.AddDays(-30);
        var hasta = fechaHasta ?? hoy;

        desde = DateTime.SpecifyKind(
            desde.Date, DateTimeKind.Utc);

        hasta = DateTime.SpecifyKind(
            hasta.Date, DateTimeKind.Utc);

        if (desde > hasta)
        {
            return BadRequest(
                "La fecha inicial no puede ser posterior a la fecha final.");
        }

        if (cantidad <= 0)
        {
            return BadRequest(
                "La cantidad debe ser mayor que cero.");
        }

        var productos = await _obtenerProductosMasVendidos.EjecutarAsync(
            desde,
            hasta.AddDays(1).AddTicks(-1),
            cantidad);

        return View(productos);
    }


    [HttpGet]
    public async Task<IActionResult> InventarioActual()
    {
        var productos = await _obtenerInventario.EjecutarAsync();

        return View(productos);
    }


    [HttpGet]
    public async Task<IActionResult> MovimientosStock(
        DateTime? fechaDesde,
        DateTime? fechaHasta,
        string periodo = "personalizado",
        DateTime? fechaSemana = null)
    {
        var zonaArgentina = TimeZoneInfo.FindSystemTimeZoneById(
            "America/Argentina/Buenos_Aires");

        var hoyArgentina = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.UtcNow,
            zonaArgentina).Date;

        DateTime desde;
        DateTime hasta;

        // Calcula el lunes de la semana que contiene una fecha.
        static DateTime ObtenerLunes(DateTime fecha)
        {
            int diasDesdeLunes =
                ((int)fecha.DayOfWeek + 6) % 7;

            return fecha.Date.AddDays(-diasDesdeLunes);
        }

        switch (periodo)
        {
            case "estaSemana":
                desde = ObtenerLunes(hoyArgentina);
                hasta = desde.AddDays(6);
                break;

            case "semanaAnterior":
                desde = ObtenerLunes(hoyArgentina).AddDays(-7);
                hasta = desde.AddDays(6);
                break;

            case "elegirSemana":
                var fechaSeleccionada =
                    fechaSemana?.Date ?? hoyArgentina;

                desde = ObtenerLunes(fechaSeleccionada);
                hasta = desde.AddDays(6);
                break;

            case "personalizado":
                desde = fechaDesde?.Date ?? hoyArgentina.AddDays(-30);
                hasta = fechaHasta?.Date ?? hoyArgentina;
                break;

            default:
                return BadRequest("El período seleccionado no es válido.");
        }

        if (desde > hasta)
        {
            return BadRequest(
                "La fecha inicial no puede superar la fecha final.");
        }

        if ((hasta - desde).TotalDays > 366)
        {
            return BadRequest(
                "El período no puede superar 366 días.");
        }

        // Interpretar las fechas como días de Argentina.
        var inicioUtc = TimeZoneInfo.ConvertTimeToUtc(
            DateTime.SpecifyKind(
                desde, DateTimeKind.Unspecified),
            zonaArgentina);

        var finUtcExclusivo = TimeZoneInfo.ConvertTimeToUtc(
            DateTime.SpecifyKind(
                hasta.AddDays(1), DateTimeKind.Unspecified),
            zonaArgentina);

        var movimientos = await _obtenerMovimientos.EjecutarAsync(
            inicioUtc,
            finUtcExclusivo);

        ViewBag.FechaDesde = desde;
        ViewBag.FechaHasta = hasta;
        ViewBag.Periodo = periodo;
        ViewBag.FechaSemana = fechaSemana?.Date ?? hoyArgentina;

        return View(movimientos);
    }



    [HttpGet]
    public async Task<IActionResult> DescargarMovimientosStockPdf(
        DateTime? fechaDesde,
        DateTime? fechaHasta,
        string periodo = "personalizado",
        DateTime? fechaSemana = null)
    {
        var zonaArgentina = TimeZoneInfo.FindSystemTimeZoneById(
            "America/Argentina/Buenos_Aires");

        var hoyArgentina = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.UtcNow,
            zonaArgentina).Date;

        static DateTime ObtenerLunes(DateTime fecha)
        {
            int diasDesdeLunes =
                ((int)fecha.DayOfWeek + 6) % 7;

            return fecha.Date.AddDays(-diasDesdeLunes);
        }

        DateTime desde;
        DateTime hasta;

        switch (periodo)
        {
            case "estaSemana":
                desde = ObtenerLunes(hoyArgentina);
                hasta = desde.AddDays(6);
                break;

            case "semanaAnterior":
                desde = ObtenerLunes(hoyArgentina).AddDays(-7);
                hasta = desde.AddDays(6);
                break;

            case "elegirSemana":
                desde = ObtenerLunes(
                    fechaSemana?.Date ?? hoyArgentina);
                hasta = desde.AddDays(6);
                break;

            case "personalizado":
                desde = fechaDesde?.Date ?? hoyArgentina.AddDays(-30);
                hasta = fechaHasta?.Date ?? hoyArgentina;
                break;

            default:
                return BadRequest(
                    "El período seleccionado no es válido.");
        }

        if (desde > hasta)
        {
            return BadRequest(
                "La fecha inicial no puede superar la fecha final.");
        }

        if ((hasta - desde).TotalDays > 366)
        {
            return BadRequest(
                "El período no puede superar 366 días.");
        }

        var inicioUtc = TimeZoneInfo.ConvertTimeToUtc(
            DateTime.SpecifyKind(
                desde, DateTimeKind.Unspecified),
            zonaArgentina);

        var finUtcExclusivo = TimeZoneInfo.ConvertTimeToUtc(
            DateTime.SpecifyKind(
                hasta.AddDays(1), DateTimeKind.Unspecified),
            zonaArgentina);

        var movimientos = await _obtenerMovimientos.EjecutarAsync(
            inicioUtc,
            finUtcExclusivo);

        var pdf = _generadorMovimientosPdf.Generar(
            movimientos,
            desde,
            hasta);

        var nombreArchivo =
            $"MovimientosStock_{desde:yyyyMMdd}_{hasta:yyyyMMdd}.pdf";

        return File(
            pdf,
            "application/pdf",
            nombreArchivo);
    }


}
