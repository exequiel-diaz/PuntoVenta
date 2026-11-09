
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


    public CuentasCorrientesController(
        ListarCuentasCorrientes listarCuentas, 
        ObtenerDetalleCuentaCorriente obtenerDetalle)
    {
        _listarCuentas = listarCuentas;
        _obtenerDetalle = obtenerDetalle;
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

        return View(cuenta);
    }


}
