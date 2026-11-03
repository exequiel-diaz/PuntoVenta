using Microsoft.AspNetCore.Mvc;
using SistemaVenta.Application.UseCases.Pagos;
using SistemaVenta.Application.UseCases.CuentasCorrientes;
using System.Security.Claims;
using SistemaVenta.Web.ViewModels.Pagos;
using SistemaVenta.Application.UseCases.Ventas;

namespace SistemaVenta.Web.Controllers
{
    public class PagosController : Controller
    {
        private readonly ObtenerDetallePago _obtenerDetallePago;
        private readonly RegistrarCobroVentaCuentaCorriente _registrarCobroVenta;
        private readonly ObtenerDetalleVenta _obtenerDetalleVenta;
        public PagosController(
            ObtenerDetallePago obtenerDetallePago,
            RegistrarCobroVentaCuentaCorriente registrarCobroVenta,
            ObtenerDetalleVenta obtenerDetalleVenta)
        {
            _obtenerDetallePago = obtenerDetallePago;
            _registrarCobroVenta = registrarCobroVenta;
            _obtenerDetalleVenta = obtenerDetalleVenta;
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
        public async Task<IActionResult> RegistrarCobroVenta(int ventaId)
        {
            if (ventaId <= 0)
            {
                return BadRequest("La venta debe ser válida.");
            }

            var venta = await _obtenerDetalleVenta.EjecutarAsync(ventaId);

            if (venta is null)
            {
                return NotFound();
            }

            if (venta.Estado != SistemaVenta.Domain.Enums.EstadoVenta.Confirmada)
            {
                return BadRequest("La venta no está confirmada.");
            }

            if (!venta.EsVentaCuentaCorriente || venta.SaldoPendiente <= 0)
            {
                return BadRequest(
                    "La venta no tiene una deuda pendiente en Cuenta Corriente.");
            }

            var modelo = new RegistrarCobroVentaViewModel
            {
                VentaId = venta.Id,
                SaldoPendiente = venta.SaldoPendiente
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
                var ventaConsultada = await _obtenerDetalleVenta
                    .EjecutarAsync(modelo.VentaId);

                if (ventaConsultada is not null)
                {
                    modelo.SaldoPendiente = ventaConsultada.SaldoPendiente;
                }

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

            var venta = await _obtenerDetalleVenta.EjecutarAsync(modelo.VentaId);

            if (venta is not null)
            {
                modelo.SaldoPendiente = venta.SaldoPendiente;
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