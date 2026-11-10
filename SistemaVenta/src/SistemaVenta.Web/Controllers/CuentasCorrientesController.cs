
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaVenta.Application.UseCases.CuentasCorrientes;
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Web.Controllers;

[Authorize(Roles = nameof(RolUsuario.Administrador))]
public class CuentasCorrientesController : Controller
{
    private readonly ListarCuentasCorrientes _listarCuentas;
    private readonly ObtenerDetalleCuentaCorriente _obtenerDetalle;
    private readonly ListarVentasPendientesCuentaCorriente _listarPendientes;

    public CuentasCorrientesController(
        ListarCuentasCorrientes listarCuentas,
        ObtenerDetalleCuentaCorriente obtenerDetalle,
        ListarVentasPendientesCuentaCorriente listarPendientes)
    {
        _listarCuentas = listarCuentas;
        _obtenerDetalle = obtenerDetalle;
        _listarPendientes = listarPendientes;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var cuentas = await _listarCuentas.EjecutarAsync();

        return View(cuentas);
    }

    [HttpGet]
    public async Task<IActionResult> Detalle(int id)
    {
        if (id <= 0)
            return BadRequest();

        var cuenta = await _obtenerDetalle.EjecutarAsync(id);

        if (cuenta is null)
            return NotFound();

        var ventasPendientes =
            await _listarPendientes.EjecutarAsync(id);

        ViewData["VentasPendientes"] = ventasPendientes;

        return View(cuenta);
    }
}
