
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Application.UseCases.ConfiguracionCredito;
using SistemaVenta.Web.Security;
using SistemaVenta.Web.ViewModels.ConfiguracionCredito;

namespace SistemaVenta.Web.Controllers;

[Authorize(Roles = Roles.Administrador)]
public class ConfiguracionCreditoController : Controller
{
    private readonly IConfiguracionCreditoRepository _repositorio;
    private readonly CambiarLimitePredeterminado _cambiarLimite;

    public ConfiguracionCreditoController(
        IConfiguracionCreditoRepository repositorio,
        CambiarLimitePredeterminado cambiarLimite)
    {
        _repositorio = repositorio;
        _cambiarLimite = cambiarLimite;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var limite = await _repositorio
            .ObtenerLimitePredeterminadoAsync();

        var modelo = new ConfiguracionCreditoViewModel
        {
            LimitePredeterminado = limite
        };

        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(
        ConfiguracionCreditoViewModel modelo)
    {
        if (!ModelState.IsValid)
            return View(modelo);

        try
        {
            await _cambiarLimite.EjecutarAsync(
                modelo.LimitePredeterminado);

            TempData["MensajeExito"] =
                "El límite predeterminado se actualizó correctamente.";

            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);
        }

        return View(modelo);
    }
}
