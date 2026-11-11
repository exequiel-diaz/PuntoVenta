
using Microsoft.AspNetCore.Mvc;
using SistemaVenta.Application.DTOs.Dashboard;
using SistemaVenta.Application.UseCases.Dashboard;
using SistemaVenta.Web.Models;
using SistemaVenta.Web.Security;
using System.Diagnostics;

namespace SistemaVenta.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ObtenerResumenDashboard _obtenerResumen;

    public HomeController(
        ILogger<HomeController> logger,
        ObtenerResumenDashboard obtenerResumen)
    {
        _logger = logger;
        _obtenerResumen = obtenerResumen;
    }

    public async Task<IActionResult> Index()
    {
        if (!User.IsInRole(Roles.Administrador))
        {
            return View(new ResumenDashboardDto());
        }

        var zonaHoraria = TimeZoneInfo.FindSystemTimeZoneById(
            "America/Argentina/Buenos_Aires");

        var ahoraLocal = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.UtcNow,
            zonaHoraria);

        var resumen = await _obtenerResumen.EjecutarAsync(
            ahoraLocal,
            zonaHoraria);

        return View(resumen);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(
        Duration = 0,
        Location = ResponseCacheLocation.None,
        NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id
                ?? HttpContext.TraceIdentifier
        });
    }
}
