
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
    private readonly IGeneradorInformeInventarioPdf _generadorInventarioPdf;
    private readonly ObtenerMejoresClientes _obtenerMejoresClientes;
    private readonly ObtenerInformeRentabilidad _obtenerRentabilidad;


    public InformesController(
        ObtenerInformeVentasPorPeriodo obtenerInformeVentas, 
        ObtenerProductosMasVendidos obtenerProductosMasVendidos,
        ObtenerVentasDiarias obtenerVentasDiarias,
        ObtenerVentasPorVendedor obtenerVentasPorVendedor,
        ObtenerInformeInventario obtenerInventario,
        ObtenerInformeMovimientosStock obtenerMovimientos,
        IGeneradorInformeMovimientosPdf generadorMovimientosPdf,
        IGeneradorInformeInventarioPdf generadorInventarioPdf,
        ObtenerMejoresClientes obtenerMejoresClientes,
        ObtenerInformeRentabilidad obtenerRentabilidad)
    {
        _obtenerInformeVentas = obtenerInformeVentas;
        _obtenerProductosMasVendidos = obtenerProductosMasVendidos;
        _obtenerVentasDiarias = obtenerVentasDiarias;
        _obtenerVentasPorVendedor = obtenerVentasPorVendedor;
        _obtenerInventario = obtenerInventario;
        _obtenerMovimientos = obtenerMovimientos;
        _generadorMovimientosPdf = generadorMovimientosPdf;
        _generadorInventarioPdf = generadorInventarioPdf;
        _obtenerMejoresClientes = obtenerMejoresClientes;
        _obtenerRentabilidad = obtenerRentabilidad;
    }



    [HttpGet]
    public async Task<IActionResult> Index(
        DateTime? fechaDesde,
        DateTime? fechaHasta)
    {
        var zonaArgentina = TimeZoneInfo.FindSystemTimeZoneById(
            "America/Argentina/Buenos_Aires");

        var hoyArgentina = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.UtcNow,
            zonaArgentina).Date;

        var desde = fechaDesde?.Date
            ?? hoyArgentina.AddDays(-30);

        var hasta = fechaHasta?.Date
            ?? hoyArgentina;

        if (desde > hasta)
        {
            return BadRequest(
                "La fecha inicial no puede ser posterior a la final.");
        }

        // Las fechas del formulario representan días argentinos.
        var inicioLocal = DateTime.SpecifyKind(
            desde, DateTimeKind.Unspecified);

        var finLocalExclusivo = DateTime.SpecifyKind(
            hasta.AddDays(1), DateTimeKind.Unspecified);

        var inicioUtc = TimeZoneInfo.ConvertTimeToUtc(
            inicioLocal, zonaArgentina);

        var finUtcInclusivo = TimeZoneInfo.ConvertTimeToUtc(
            finLocalExclusivo, zonaArgentina).AddTicks(-1);

        // Consultas existentes.
        var resumenVentas = await _obtenerInformeVentas
            .EjecutarAsync(inicioUtc, finUtcInclusivo);

        var ventasDiarias = await _obtenerVentasDiarias
            .EjecutarAsync(inicioUtc, finUtcInclusivo);

        var ventasPorVendedor = await _obtenerVentasPorVendedor
            .EjecutarAsync(inicioUtc, finUtcInclusivo);

        // Nuevos rankings del dashboard.
        var productosDestacados = await _obtenerProductosMasVendidos
            .EjecutarAsync(
                inicioUtc,
                finUtcInclusivo,
                5,
                "cantidad");

        var mejoresClientes = await _obtenerMejoresClientes
            .EjecutarAsync(
                inicioUtc,
                finUtcInclusivo,
                5);


        // Rentabilidad del período seleccionado.
        var rentabilidad = await _obtenerRentabilidad.EjecutarAsync(
            inicioUtc,
            finUtcInclusivo,
            5);


        var modelo = new DashboardInformesViewModel
        {
            FechaDesde = desde,
            FechaHasta = hasta,

            ResumenVentas = resumenVentas,
            VentasDiarias = ventasDiarias,
            VentasPorVendedor = ventasPorVendedor,

            ProductosDestacados = productosDestacados,
            MejoresClientes = mejoresClientes,
            Rentabilidad = rentabilidad
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
    int cantidad = 10,
    string criterio = "cantidad")
    {
        var zona = TimeZoneInfo.FindSystemTimeZoneById(
            "America/Argentina/Buenos_Aires");

        var hoyArgentina = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.UtcNow, zona).Date;

        var desde = fechaDesde?.Date ?? hoyArgentina.AddDays(-30);
        var hasta = fechaHasta?.Date ?? hoyArgentina;

        if (desde > hasta)
            return BadRequest(
                "La fecha inicial no puede ser posterior a la final.");

        if (cantidad is not (5 or 10 or 20 or 50))
            return BadRequest(
                "La cantidad debe ser 5, 10, 20 o 50.");

        if (criterio != "cantidad" && criterio != "facturacion")
            return BadRequest(
                "Criterio de ordenamiento no válido.");

        var inicioLocal = DateTime.SpecifyKind(
            desde, DateTimeKind.Unspecified);

        var finLocalExclusivo = DateTime.SpecifyKind(
            hasta.AddDays(1), DateTimeKind.Unspecified);

        var inicioUtc = TimeZoneInfo.ConvertTimeToUtc(
            inicioLocal, zona);

        var finUtcInclusivo = TimeZoneInfo.ConvertTimeToUtc(
            finLocalExclusivo, zona).AddTicks(-1);

        var productos = await _obtenerProductosMasVendidos.EjecutarAsync(
            inicioUtc,
            finUtcInclusivo,
            cantidad,
            criterio);

        ViewData["FechaDesde"] = desde.ToString("yyyy-MM-dd");
        ViewData["FechaHasta"] = hasta.ToString("yyyy-MM-dd");
        ViewData["Cantidad"] = cantidad;
        ViewData["Criterio"] = criterio;

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


    [HttpGet]
    public async Task<IActionResult> DescargarInventarioActualPdf()
    {
        var productos = await _obtenerInventario.EjecutarAsync();

        var pdf = _generadorInventarioPdf.Generar(productos);

        var nombreArchivo =
            $"InventarioActual_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf";

        return File(
            pdf,
            "application/pdf",
            nombreArchivo);
    }



    [HttpGet]
    public async Task<IActionResult> MejoresClientes(
        DateTime? fechaDesde,
        DateTime? fechaHasta,
        int cantidad = 10)
    {
        var zonaArgentina = TimeZoneInfo.FindSystemTimeZoneById(
            "America/Argentina/Buenos_Aires");

        var hoyArgentina = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.UtcNow, zonaArgentina).Date;

        var desde = fechaDesde?.Date
            ?? hoyArgentina.AddDays(-30);

        var hasta = fechaHasta?.Date
            ?? hoyArgentina;

        if (desde > hasta)
        {
            return BadRequest(
                "La fecha inicial no puede ser posterior a la final.");
        }

        if (cantidad is not (5 or 10 or 20 or 50))
        {
            return BadRequest(
                "La cantidad debe ser 5, 10, 20 o 50.");
        }

        var inicioLocal = DateTime.SpecifyKind(
            desde, DateTimeKind.Unspecified);

        var finLocalExclusivo = DateTime.SpecifyKind(
            hasta.AddDays(1), DateTimeKind.Unspecified);

        var inicioUtc = TimeZoneInfo.ConvertTimeToUtc(
            inicioLocal, zonaArgentina);

        var finUtcInclusivo = TimeZoneInfo.ConvertTimeToUtc(
            finLocalExclusivo, zonaArgentina).AddTicks(-1);

        var clientes = await _obtenerMejoresClientes.EjecutarAsync(
            inicioUtc,
            finUtcInclusivo,
            cantidad);

        ViewData["FechaDesde"] = desde.ToString("yyyy-MM-dd");
        ViewData["FechaHasta"] = hasta.ToString("yyyy-MM-dd");
        ViewData["Cantidad"] = cantidad;

        return View(clientes);
    }



    [HttpGet]
    public async Task<IActionResult> RankingVendedores(
        DateTime? fechaDesde,
        DateTime? fechaHasta,
        int cantidad = 10,
        string criterio = "facturacion")
    {
        var zonaArgentina = TimeZoneInfo.FindSystemTimeZoneById(
            "America/Argentina/Buenos_Aires");

        var hoyArgentina = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.UtcNow, zonaArgentina).Date;

        var desde = fechaDesde?.Date
            ?? hoyArgentina.AddDays(-30);

        var hasta = fechaHasta?.Date
            ?? hoyArgentina;

        if (desde > hasta)
        {
            return BadRequest(
                "La fecha inicial no puede ser posterior a la final.");
        }

        if (cantidad is not (5 or 10 or 20 or 50))
        {
            return BadRequest(
                "La cantidad debe ser 5, 10, 20 o 50.");
        }

        if (criterio != "facturacion" &&
            criterio != "cantidad")
        {
            return BadRequest(
                "Criterio de ordenamiento no válido.");
        }

        var inicioLocal = DateTime.SpecifyKind(
            desde, DateTimeKind.Unspecified);

        var finLocalExclusivo = DateTime.SpecifyKind(
            hasta.AddDays(1), DateTimeKind.Unspecified);

        var inicioUtc = TimeZoneInfo.ConvertTimeToUtc(
            inicioLocal, zonaArgentina);

        var finUtcInclusivo = TimeZoneInfo.ConvertTimeToUtc(
            finLocalExclusivo, zonaArgentina).AddTicks(-1);

        var vendedores = await _obtenerVentasPorVendedor
            .EjecutarAsync(inicioUtc, finUtcInclusivo);

        // Las ventas sin vendedor asignado no participan
        // del ranking individual de vendedores.
        var vendedoresIdentificados = vendedores
            .Where(v => v.VendedorId.HasValue);

        var ranking = criterio == "cantidad"
            ? vendedoresIdentificados
                .OrderByDescending(v => v.CantidadVentas)
                .ThenByDescending(v => v.TotalVendido)
                .Take(cantidad)
                .ToList()
            : vendedoresIdentificados
                .OrderByDescending(v => v.TotalVendido)
                .ThenByDescending(v => v.CantidadVentas)
                .Take(cantidad)
                .ToList();

        ViewData["FechaDesde"] = desde.ToString("yyyy-MM-dd");
        ViewData["FechaHasta"] = hasta.ToString("yyyy-MM-dd");
        ViewData["Cantidad"] = cantidad;
        ViewData["Criterio"] = criterio;

        return View(ranking);
    }


    [HttpGet]
    public async Task<IActionResult> Rentabilidad(
        DateTime? fechaDesde,
        DateTime? fechaHasta,
        int top = 10)
    {
        var zona = TimeZoneInfo.FindSystemTimeZoneById(
            "America/Argentina/Buenos_Aires");

        var hoy = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.UtcNow, zona).Date;

        var desde = fechaDesde?.Date ?? hoy.AddDays(-30);
        var hasta = fechaHasta?.Date ?? hoy;

        if (desde > hasta)
            return BadRequest(
                "La fecha inicial no puede superar la final.");

        if (hasta == DateTime.MaxValue.Date)
            return BadRequest("La fecha final no es válida.");

        if (top is not (5 or 10 or 20 or 50))
            return BadRequest("Top no válido.");

        var inicioUtc = TimeZoneInfo.ConvertTimeToUtc(
            DateTime.SpecifyKind(
                desde, DateTimeKind.Unspecified),
            zona);

        var finUtcExclusivo = TimeZoneInfo.ConvertTimeToUtc(
            DateTime.SpecifyKind(
                hasta.AddDays(1), DateTimeKind.Unspecified),
            zona);

        var modelo = await _obtenerRentabilidad.EjecutarAsync(
            inicioUtc,
            finUtcExclusivo.AddTicks(-1),
            top);

        ViewData["FechaDesde"] = desde.ToString("yyyy-MM-dd");
        ViewData["FechaHasta"] = hasta.ToString("yyyy-MM-dd");
        ViewData["Top"] = top;

        return View(modelo);
    }


}
