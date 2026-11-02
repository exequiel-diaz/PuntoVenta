
using Microsoft.AspNetCore.Mvc;
using SistemaVenta.Application.UseCases.DocumentosComerciales;

namespace SistemaVenta.Web.Controllers;

public class DocumentosComercialesController : Controller
{
    private readonly EmitirNotaCredito _emitirNotaCredito;
    private readonly EmitirNotaDebito _emitirNotaDebito;

    public DocumentosComercialesController(
        EmitirNotaCredito emitirNotaCredito,
        EmitirNotaDebito emitirNotaDebito)
    {
        _emitirNotaCredito = emitirNotaCredito;
        _emitirNotaDebito = emitirNotaDebito;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EmitirNotaCredito(
        int facturaId,
        string motivo,
        decimal importeTotal)
    {
        try
        {
            await _emitirNotaCredito.EjecutarAsync(
                facturaId, motivo, importeTotal);

            TempData["MensajeExito"] =
                "La Nota de Crédito se emitió correctamente.";
        }
        catch (ArgumentException ex)
        {
            TempData["MensajeError"] = ex.Message;
        }
        catch (InvalidOperationException ex)
        {
            TempData["MensajeError"] = ex.Message;
        }

        return RedirectToAction("Index", "Ventas");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EmitirNotaDebito(
        int facturaId,
        string motivo,
        decimal importeTotal)
    {
        try
        {
            await _emitirNotaDebito.EjecutarAsync(
                facturaId, motivo, importeTotal);

            TempData["MensajeExito"] =
                "La Nota de Débito se emitió correctamente.";
        }
        catch (ArgumentException ex)
        {
            TempData["MensajeError"] = ex.Message;
        }
        catch (InvalidOperationException ex)
        {
            TempData["MensajeError"] = ex.Message;
        }

        return RedirectToAction("Index", "Ventas");
    }
}
