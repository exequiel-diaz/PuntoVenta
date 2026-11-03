using Microsoft.AspNetCore.Mvc;
using SistemaVenta.Application.UseCases.Pagos;
using SistemaVenta.Application.UseCases.CuentasCorrientes;
using System.Security.Claims;
using SistemaVenta.Web.ViewModels.Pagos;

namespace SistemaVenta.Web.Controllers
{
    public class PagosController : Controller
    {
        private readonly ObtenerDetallePago _obtenerDetallePago;
        private readonly RegistrarCobroVentaCuentaCorriente _registrarCobroVenta;

        public PagosController(
            ObtenerDetallePago obtenerDetallePago,
            RegistrarCobroVentaCuentaCorriente registrarCobroVenta)
        {
            _obtenerDetallePago = obtenerDetallePago;
            _registrarCobroVenta = registrarCobroVenta;
        }

        [HttpGet]
        public async Task<IActionResult> Detalle(int id)
        {
            var pago = await _obtenerDetallePago.EjecutarAsync(id);

            if (pago is null)
                return NotFound();

            return View(pago);
        }
        
        [HttpGet]
        public IActionResult RegistrarCobroVenta(int ventaId)
        {
            if (ventaId <= 0)
            {
                return BadRequest(
                    "La venta debe ser válida.");
            }

            var modelo = new RegistrarCobroVentaViewModel
            {
                VentaId = ventaId
            };

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarCobroVenta(
    RegistrarCobroVentaViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            var usuarioId = ObtenerUsuarioId();

            if (!usuarioId.HasValue)
            {
                return Challenge();
            }

            try
            {
                var pagoId = await _registrarCobroVenta.EjecutarAsync(
                    modelo.VentaId,
                    modelo.Importe,
                    modelo.FormaPago!.Value,
                    usuarioId.Value,
                    modelo.Observaciones);

                return RedirectToAction(
                    nameof(Detalle),
                    new { id = pagoId });
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


        private int? ObtenerUsuarioId()
        {
            var identificador = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (int.TryParse(identificador, out var usuarioId)
                && usuarioId > 0)
            {
                return usuarioId;
            }

            return null;
        }
    }
}