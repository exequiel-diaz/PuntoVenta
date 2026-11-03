
using Microsoft.AspNetCore.Mvc;
using SistemaVenta.Application.UseCases.Facturas;
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Web.Controllers;

public class FacturasController : Controller
{
    private readonly EmitirFactura _emitirFactura;
    private readonly ObtenerDetalleFactura _obtenerDetalleFactura;

    public FacturasController(EmitirFactura emitirFactura, 
        ObtenerDetalleFactura obtenerDetalleFactura)
    {
        _emitirFactura = emitirFactura;
        _obtenerDetalleFactura = obtenerDetalleFactura;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Emitir(
        int ventaId,
        TipoFactura tipo)
    {
        try
        {
            var facturaId = await _emitirFactura.EjecutarAsync(
                ventaId,
                tipo);

            TempData["MensajeExito"] =
                $"La factura interna #{facturaId} " +
                "se emitió correctamente.";
        }
        catch (ArgumentException ex)
        {
            TempData["MensajeError"] = ex.Message;
        }
        catch (InvalidOperationException ex)
        {
            TempData["MensajeError"] = ex.Message;
        }

        return RedirectToAction(
            "Detalle",
            "Ventas",
            new { id = ventaId });
    }


    [HttpGet]
    public async Task<IActionResult> Detalle(int id)
    {
        var factura = await _obtenerDetalleFactura.EjecutarAsync(id);

        if (factura is null)
            return NotFound();

        return View(factura);
    }

}
