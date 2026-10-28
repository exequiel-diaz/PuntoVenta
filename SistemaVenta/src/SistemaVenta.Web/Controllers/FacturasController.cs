
using Microsoft.AspNetCore.Mvc;
using SistemaVenta.Application.UseCases.Facturas;
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Web.Controllers;

public class FacturasController : Controller
{
    private readonly EmitirFactura _emitirFactura;

    public FacturasController(EmitirFactura emitirFactura)
    {
        _emitirFactura = emitirFactura;
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
}
